using AutoMapper;
using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.DAL.Entities;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Invoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InvoiceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<InvoiceDto>> GetInvoicesByContractAsync(int contractId)
        {
            var invoices = await _unitOfWork.Invoices.FindAsync(i => i.ContractID == contractId, "Contract.Room");
            return _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        }

        public async Task<InvoiceDetailDto> GetInvoiceByIdAsync(int id)
        {
            var invoice = await _unitOfWork.Invoices.FirstOrDefaultAsync(i => i.InvoiceID == id, "Contract.Room, Contract.Tenant, MeterReading");
            if (invoice == null) throw new NotFoundException("Hóa đơn", id);
            return _mapper.Map<InvoiceDetailDto>(invoice);
        }

        public async Task<InvoiceDto> GenerateMonthlyInvoiceAsync(GenerateInvoiceDto request)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var contract = await _unitOfWork.Contracts.GetByIdAsync(request.ContractID);
                if (contract == null || contract.Status != "Active")
                    throw new BadRequestException("Hợp đồng không hợp lệ hoặc không còn hiệu lực.");

                var lastReading = (await _unitOfWork.MeterReadings.FindAsync(m => m.ContractID == request.ContractID))
                    .OrderByDescending(m => m.ReadingMonth).FirstOrDefault();
                decimal oldElectric = lastReading?.NewElectric ?? 0;
                decimal oldWater = lastReading?.NewWater ?? 0;

                if (request.NewElectric < oldElectric)
                    throw new BadRequestException($"Chỉ số điện mới ({request.NewElectric}) không được nhỏ hơn chỉ số cũ ({oldElectric}).");
                if (request.NewWater < oldWater)
                    throw new BadRequestException($"Chỉ số nước mới ({request.NewWater}) không được nhỏ hơn chỉ số cũ ({oldWater}).");

                var reading = new MeterReading
                {
                    ContractID = request.ContractID,
                    ReadingMonth = request.ReadingMonth,
                    OldElectric = oldElectric,
                    NewElectric = request.NewElectric,
                    OldWater = oldWater,
                    NewWater = request.NewWater,
                    CreatedBy = request.CreatedBy,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                await _unitOfWork.MeterReadings.AddAsync(reading);
                await _unitOfWork.SaveChangesAsync();

                decimal electricCost = (request.NewElectric - oldElectric) * contract.ElectricPrice;
                decimal waterCost = (request.NewWater - oldWater) * contract.WaterPrice;
                decimal total = contract.MonthlyRent + electricCost + waterCost + request.OtherFee;

                var invoice = new Invoice
                {
                    InvoiceCode = "INV" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    ContractID = contract.ContractID,
                    ReadingID = reading.ReadingID,
                    Rent = contract.MonthlyRent,
                    ElectricCost = electricCost,
                    WaterCost = waterCost,
                    OtherFee = request.OtherFee,
                    Total = total,
                    Status = "Unpaid",
                    DueDate = request.ReadingMonth.AddDays(5),
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                await _unitOfWork.Invoices.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    UserID = contract.TenantID,
                    Title = "Hóa đơn mới",
                    Content = $"Hóa đơn {invoice.InvoiceCode} đã được tạo. Tổng tiền: {invoice.Total:N0} đ. Hạn thanh toán: {invoice.DueDate:dd/MM/yyyy}.",
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var createdInvoice = await _unitOfWork.Invoices.FirstOrDefaultAsync(i => i.InvoiceID == invoice.InvoiceID, "Contract.Room");
                return _mapper.Map<InvoiceDto>(createdInvoice);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> ProcessPaymentAsync(int invoiceId, ProcessPaymentDto request)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
                if (invoice == null) throw new NotFoundException("Hóa đơn", invoiceId);
                if (invoice.Status == "Paid")
                    throw new BadRequestException("Hóa đơn này đã được thanh toán.");
                if (request.Amount < invoice.Total)
                    throw new BadRequestException("Số tiền thanh toán không đủ.");

                var payment = new Payment
                {
                    InvoiceID = invoiceId,
                    PaymentDate = DateTime.Now,
                    Amount = request.Amount,
                    Method = request.Method,
                    Status = "Completed",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                await _unitOfWork.Payments.AddAsync(payment);

                invoice.Status = "Paid";
                invoice.PaidDate = DateTime.Now;
                invoice.UpdatedDate = DateTime.Now;
                _unitOfWork.Invoices.Update(invoice);

                var contract = await _unitOfWork.Contracts.FirstOrDefaultAsync(c => c.ContractID == invoice.ContractID, "Room.House");
                if (contract?.Room?.House != null)
                {
                    await _unitOfWork.Notifications.AddAsync(new Notification
                    {
                        UserID = contract.Room.House.OwnerID,
                        Title = "Thanh toán hóa đơn",
                        Content = $"Hóa đơn {invoice.InvoiceCode} đã được thanh toán {request.Amount:N0} đ ({request.Method}).",
                        IsRead = false,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    });
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}