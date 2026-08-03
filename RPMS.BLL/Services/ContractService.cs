using AutoMapper;
using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.DAL.Entities;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class ContractService : IContractService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ContractService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ContractDto>> GetAllContractsAsync()
        {
            var contracts = await _unitOfWork.Contracts.GetAllAsync("Room, Tenant");
            return _mapper.Map<IEnumerable<ContractDto>>(contracts);
        }

        public async Task<IEnumerable<ContractDto>> GetContractsByTenantAsync(int tenantId)
        {
            var contracts = await _unitOfWork.Contracts.FindAsync(c => c.TenantID == tenantId, "Room, Tenant");
            return _mapper.Map<IEnumerable<ContractDto>>(contracts);
        }

        public async Task<IEnumerable<ContractDto>> GetContractsByLandlordAsync(int landlordId)
        {
            var contracts = await _unitOfWork.Contracts.FindAsync(
                c => c.Room.House.OwnerID == landlordId,
                "Room.House, Tenant");
            return _mapper.Map<IEnumerable<ContractDto>>(contracts);
        }

        public async Task<IEnumerable<ContractDto>> GetContractsByManagerAsync(int managerId)
        {
            var assignments = await _unitOfWork.Assignments.FindAsync(a => a.ManagerID == managerId && a.Status == "Active");
            var houseIds = assignments.Select(a => a.HouseID).ToList();
            if (houseIds.Count == 0)
                return Array.Empty<ContractDto>();

            var contracts = await _unitOfWork.Contracts.FindAsync(
                c => houseIds.Contains(c.Room.HouseID),
                "Room.House, Tenant");
            return _mapper.Map<IEnumerable<ContractDto>>(contracts);
        }

        public async Task<ContractDetailDto> GetContractByIdAsync(int id)
        {
            var contract = await _unitOfWork.Contracts.FirstOrDefaultAsync(c => c.ContractID == id, "Room, Tenant, CreatedByUser");
            if (contract == null) throw new NotFoundException("Hợp đồng", id);
            return _mapper.Map<ContractDetailDto>(contract);
        }

        public async Task<ContractDto> CreateContractAsync(CreateContractDto request, int createdById)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var room = await _unitOfWork.Rooms.GetByIdAsync(request.RoomID);
                if (room == null) throw new NotFoundException("Phòng", request.RoomID);
                if (room.Status == "Occupied")
                    throw new BadRequestException("Phòng này đã có người thuê.");
                if (request.EndDate <= request.StartDate)
                    throw new BadRequestException("Ngày kết thúc phải lớn hơn ngày bắt đầu.");

                room.Status = "Occupied";
                room.UpdatedDate = DateTime.Now;
                _unitOfWork.Rooms.Update(room);

                var contract = new Contract
                {
                    ContractCode = "HD" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    RoomID = request.RoomID,
                    TenantID = request.TenantID,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    MoveInDate = request.StartDate,
                    Deposit = request.Deposit,
                    MonthlyRent = request.MonthlyRent,
                    ElectricPrice = request.ElectricPrice,
                    WaterPrice = request.WaterPrice,
                    Status = "Active",
                    CreatedBy = createdById,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                await _unitOfWork.Contracts.AddAsync(contract);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    UserID = request.TenantID,
                    Title = "Hợp đồng thuê mới",
                    Content = $"Bạn đã được tạo hợp đồng {contract.ContractCode} cho phòng {room.RoomNumber}.",
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var result = await _unitOfWork.Contracts.FirstOrDefaultAsync(c => c.ContractID == contract.ContractID, "Room, Tenant");
                return _mapper.Map<ContractDto>(result);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> TerminateContractAsync(int contractId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var contract = await _unitOfWork.Contracts.FirstOrDefaultAsync(c => c.ContractID == contractId, "Room");
                if (contract == null) throw new NotFoundException("Hợp đồng", contractId);
                if (contract.Status != "Active")
                    throw new BadRequestException("Chỉ có thể kết thúc hợp đồng đang có hiệu lực.");
                contract.Status = "Terminated";
                contract.MoveOutDate = DateTime.Now;
                contract.UpdatedDate = DateTime.Now;
                _unitOfWork.Contracts.Update(contract);

                if (contract.Room != null)
                {
                    contract.Room.Status = "Available";
                    contract.Room.UpdatedDate = DateTime.Now;
                    _unitOfWork.Rooms.Update(contract.Room);
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

        public async Task<bool> ExtendContractAsync(int contractId, DateTime newEndDate, int actorUserId)
        {
            var contract = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                c => c.ContractID == contractId, "Room, Tenant");
            if (contract == null) throw new NotFoundException("Hợp đồng", contractId);
            if (contract.Status != "Active")
                throw new BadRequestException("Chỉ gia hạn hợp đồng đang Active.");
            if (newEndDate.Date <= contract.EndDate.Date)
                throw new BadRequestException("Ngày kết thúc mới phải sau ngày kết thúc hiện tại.");

            var oldEnd = contract.EndDate;
            contract.EndDate = newEndDate.Date;
            contract.UpdatedDate = DateTime.Now;
            _unitOfWork.Contracts.Update(contract);

            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserID = contract.TenantID,
                Title = "Hợp đồng được gia hạn",
                Content = $"Hợp đồng {contract.ContractCode} gia hạn từ {oldEnd:dd/MM/yyyy} đến {newEndDate:dd/MM/yyyy}.",
                IsRead = false,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            });

            await _unitOfWork.ActivityLogs.AddAsync(new ActivityLog
            {
                UserID = actorUserId,
                Action = "ExtendContract",
                Details = $"Gia hạn {contract.ContractCode} → {newEndDate:dd/MM/yyyy}",
                CreatedDate = DateTime.Now
            });

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}