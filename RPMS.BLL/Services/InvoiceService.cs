using AutoMapper;
using RPMS.BLL.Exceptions;
using RPMS.BLL.Helpers;
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
            var invoices = await _unitOfWork.Invoices.FindAsync(i => i.ContractID == contractId, "Contract.Room, Payments");
            // Đồng bộ trạng thái nếu đã có payment Completed nhưng Status vẫn Unpaid (sample data lệch)
            foreach (var inv in invoices)
            {
                if (inv.Status != "Paid" && inv.Payments != null && inv.Payments.Any(p => p.Status == "Completed"))
                {
                    inv.Status = "Paid";
                    inv.PaidDate ??= inv.Payments.Where(p => p.Status == "Completed").Max(p => p.PaymentDate);
                    inv.UpdatedDate = DateTime.Now;
                    _unitOfWork.Invoices.Update(inv);
                }
            }
            if (invoices.Any(i => i.Status == "Paid" && i.Payments?.Any(p => p.Status == "Completed") == true))
                await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
        }

        public async Task<InvoiceDetailDto> GetInvoiceByIdAsync(int id)
        {
            var invoice = await _unitOfWork.Invoices.FirstOrDefaultAsync(
                i => i.InvoiceID == id,
                "Contract.Room.House, Contract.Tenant, MeterReading, Payments");
            if (invoice == null) throw new NotFoundException("Hóa đơn", id);

            if (invoice.Status != "Paid" && invoice.Payments != null && invoice.Payments.Any(p => p.Status == "Completed"))
            {
                invoice.Status = "Paid";
                invoice.PaidDate ??= invoice.Payments.Where(p => p.Status == "Completed").Max(p => p.PaymentDate);
                invoice.UpdatedDate = DateTime.Now;
                _unitOfWork.Invoices.Update(invoice);
                await _unitOfWork.SaveChangesAsync();
            }

            return EnrichProration(_mapper.Map<InvoiceDetailDto>(invoice), invoice);
        }

        private static InvoiceDetailDto EnrichProration(InvoiceDetailDto dto, Invoice invoice)
        {
            if (invoice.Contract == null) return dto;

            dto.MoveInDate = invoice.Contract.MoveInDate == default
                ? invoice.Contract.StartDate
                : invoice.Contract.MoveInDate;
            dto.MoveOutDate = invoice.Contract.MoveOutDate;

            DateTime billingMonth;
            if (invoice.MeterReading != null)
                billingMonth = invoice.MeterReading.ReadingMonth;
            else if (invoice.DueDate != default)
                billingMonth = new DateTime(invoice.DueDate.Year, invoice.DueDate.Month, 1);
            else
                billingMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            var calc = RentProrationHelper.Calculate(
                invoice.Contract.MonthlyRent,
                billingMonth,
                invoice.Contract.StartDate,
                invoice.Contract.EndDate,
                dto.MoveInDate,
                dto.MoveOutDate);

            dto.FullMonthlyRent = calc.FullMonthlyRent;
            dto.DaysInMonth = calc.DaysInMonth;
            dto.OccupiedDays = calc.OccupiedDays;
            dto.OccupancyFrom = calc.OccupancyFrom;
            dto.OccupancyTo = calc.OccupancyTo;
            dto.IsProrated = calc.IsProrated;
            dto.RentNote = calc.Note;
            return dto;
        }

        public async Task<MeterReadingSummaryDto?> GetLatestReadingAsync(int contractId)
        {
            var last = (await _unitOfWork.MeterReadings.FindAsync(m => m.ContractID == contractId))
                .OrderByDescending(m => m.ReadingMonth)
                .ThenByDescending(m => m.ReadingID)
                .FirstOrDefault();
            if (last == null) return null;
            return new MeterReadingSummaryDto
            {
                ReadingMonth = last.ReadingMonth,
                OldElectric = last.OldElectric,
                NewElectric = last.NewElectric,
                OldWater = last.OldWater,
                NewWater = last.NewWater
            };
        }

        public async Task<InvoiceDto> GenerateMonthlyInvoiceAsync(GenerateInvoiceDto request)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var contract = await _unitOfWork.Contracts.GetByIdAsync(request.ContractID);
                if (contract == null || contract.Status != "Active" || !contract.TenantID.HasValue)
                    throw new BadRequestException("Hợp đồng không hợp lệ, chưa có khách thuê, hoặc không còn hiệu lực.");

                var monthStart = new DateTime(request.ReadingMonth.Year, request.ReadingMonth.Month, 1);
                var monthEnd = monthStart.AddMonths(1);
                var currentMonthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                if (monthStart >= currentMonthStart)
                    throw new BadRequestException(
                        $"Chỉ được tạo hóa đơn cho tháng đã kết thúc (tháng trước trở về). " +
                        $"Không tạo hóa đơn tháng {monthStart:MM/yyyy} khi hôm nay mới {DateTime.Today:dd/MM/yyyy}.");

                var already = await _unitOfWork.MeterReadings.ExistsAsync(m =>
                    m.ContractID == request.ContractID &&
                    m.ReadingMonth >= monthStart &&
                    m.ReadingMonth < monthEnd);
                if (already)
                    throw new BadRequestException($"Đã có chỉ số / hóa đơn cho tháng {monthStart:MM/yyyy}.");

                var lastReading = (await _unitOfWork.MeterReadings.FindAsync(m => m.ContractID == request.ContractID))
                    .OrderByDescending(m => m.ReadingMonth)
                    .ThenByDescending(m => m.ReadingID)
                    .FirstOrDefault();
                decimal oldElectric = lastReading?.NewElectric ?? 0;
                decimal oldWater = lastReading?.NewWater ?? 0;

                if (request.NewElectric < oldElectric)
                    throw new BadRequestException($"Chỉ số điện mới ({request.NewElectric}) không được nhỏ hơn chỉ số tháng trước ({oldElectric}).");
                if (request.NewWater < oldWater)
                    throw new BadRequestException($"Chỉ số nước mới ({request.NewWater}) không được nhỏ hơn chỉ số tháng trước ({oldWater}).");

                var reading = new MeterReading
                {
                    ContractID = request.ContractID,
                    ReadingMonth = monthStart,
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

                decimal usageE = request.NewElectric - oldElectric;
                decimal usageW = request.NewWater - oldWater;
                decimal electricCost = ContractPricingHelper.WeightedUnitCost(
                    usageE, contract.ElectricPrice, contract.PreviousElectricPrice, contract.PriceEffectiveDate, monthStart);
                decimal waterCost = ContractPricingHelper.WeightedUnitCost(
                    usageW, contract.WaterPrice, contract.PreviousWaterPrice, contract.PriceEffectiveDate, monthStart);

                // Tiền nhà theo ngày thực ở + đổi giá giữa tháng (nếu có)
                DateTime? moveIn = contract.MoveInDate == default ? contract.StartDate : contract.MoveInDate;
                DateTime? moveOut = contract.MoveOutDate;
                decimal rent = ContractPricingHelper.CalculateRent(
                    contract.MonthlyRent,
                    contract.PreviousMonthlyRent,
                    contract.PriceEffectiveDate,
                    monthStart,
                    contract.StartDate,
                    contract.EndDate,
                    moveIn,
                    moveOut);

                var rentCalc = RentProrationHelper.Calculate(
                    contract.MonthlyRent,
                    monthStart,
                    contract.StartDate,
                    contract.EndDate,
                    moveIn,
                    moveOut);

                if (rentCalc.OccupiedDays <= 0)
                    throw new BadRequestException(
                        $"Không có ngày ở trong tháng {monthStart:MM/yyyy} để tính tiền nhà " +
                        $"(nhận phòng {moveIn:dd/MM/yyyy}, trả phòng {(moveOut?.ToString("dd/MM/yyyy") ?? "chưa")}).");

                decimal total = rent + electricCost + waterCost + request.OtherFee;

                var invoice = new Invoice
                {
                    InvoiceCode = "INV" + DateTime.Now.ToString("yyMMddHHmmss"),
                    ContractID = contract.ContractID,
                    ReadingID = reading.ReadingID,
                    Rent = rent,
                    ElectricCost = electricCost,
                    WaterCost = waterCost,
                    OtherFee = request.OtherFee,
                    Total = total,
                    Status = "Unpaid",
                    DueDate = monthStart.AddMonths(1).AddDays(-1),
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                await _unitOfWork.Invoices.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                string rentNote = rentCalc.OccupiedDays < rentCalc.DaysInMonth || contract.PriceEffectiveDate.HasValue
                    ? $" Tiền nhà {rent:N0} đ ({rentCalc.OccupiedDays}/{rentCalc.DaysInMonth} ngày" +
                      (contract.PriceEffectiveDate.HasValue ? $", đổi giá từ {contract.PriceEffectiveDate:dd/MM}" : "") + ")."
                    : "";
                if (contract.TenantID.HasValue)
                {
                    await _unitOfWork.Notifications.AddAsync(new Notification
                    {
                        UserID = contract.TenantID.Value,
                        Title = "Hóa đơn mới",
                        Content = $"Hóa đơn {invoice.InvoiceCode} tháng {monthStart:MM/yyyy} đã được tạo. Tổng: {invoice.Total:N0} đ.{rentNote} Hạn TT: {invoice.DueDate:dd/MM/yyyy}.",
                        IsRead = false,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    });
                    await _unitOfWork.SaveChangesAsync();
                }
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
                // Có payment Completed sẵn (sample lệch) → đồng bộ Paid
                var existingPaid = await _unitOfWork.Payments.ExistsAsync(
                    p => p.InvoiceID == invoiceId && p.Status == "Completed");
                if (existingPaid)
                {
                    invoice.Status = "Paid";
                    invoice.PaidDate ??= DateTime.Now;
                    invoice.UpdatedDate = DateTime.Now;
                    _unitOfWork.Invoices.Update(invoice);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    return true;
                }
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