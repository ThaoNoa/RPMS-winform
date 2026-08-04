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

                var openContract = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                    c => c.RoomID == request.RoomID && (c.Status == "Active" || c.Status == "Draft"));
                if (openContract != null)
                    throw new BadRequestException("Phòng này đã có hợp đồng nháp hoặc đang hiệu lực.");

                int? tenantId = request.TenantID is > 0 ? request.TenantID : null;
                if (tenantId.HasValue)
                {
                    var tenant = await _unitOfWork.Users.GetByIdAsync(tenantId.Value);
                    if (tenant == null || tenant.Status != "Active")
                        throw new BadRequestException("Khách thuê không hợp lệ hoặc không còn hoạt động.");
                }

                bool hasTenant = tenantId.HasValue;
                if (hasTenant)
                {
                    room.Status = "Occupied";
                    room.UpdatedDate = DateTime.Now;
                    _unitOfWork.Rooms.Update(room);
                }

                var contract = new Contract
                {
                    ContractCode = "HD" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    RoomID = request.RoomID,
                    TenantID = tenantId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    MoveInDate = request.StartDate,
                    Deposit = request.Deposit,
                    MonthlyRent = request.MonthlyRent,
                    ElectricPrice = request.ElectricPrice,
                    WaterPrice = request.WaterPrice,
                    Status = hasTenant ? "Active" : "Draft",
                    CreatedBy = createdById,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                await _unitOfWork.Contracts.AddAsync(contract);
                await _unitOfWork.SaveChangesAsync();

                if (hasTenant)
                {
                    await _unitOfWork.Notifications.AddAsync(new Notification
                    {
                        UserID = tenantId!.Value,
                        Title = "Hợp đồng thuê mới",
                        Content = $"Bạn đã được tạo hợp đồng {contract.ContractCode} cho phòng {room.RoomNumber}.",
                        IsRead = false,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    });
                    await _unitOfWork.SaveChangesAsync();
                }

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

        public async Task<ContractDto> AssignTenantAsync(AssignTenantDto request, int actorUserId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var contract = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                    c => c.ContractID == request.ContractID, "Room, Tenant");
                if (contract == null) throw new NotFoundException("Hợp đồng", request.ContractID);
                if (contract.TenantID.HasValue)
                    throw new BadRequestException("Hợp đồng đã có khách thuê.");
                if (contract.Status != "Draft" && contract.Status != "Active")
                    throw new BadRequestException("Chỉ gán khách cho hợp đồng nháp hoặc đang hiệu lực.");

                var tenant = await _unitOfWork.Users.GetByIdAsync(request.TenantID);
                if (tenant == null || tenant.Status != "Active")
                    throw new BadRequestException("Khách thuê không hợp lệ hoặc không còn hoạt động.");

                if (contract.Room == null)
                    throw new NotFoundException("Phòng", contract.RoomID);
                if (contract.Room.Status == "Occupied")
                {
                    var other = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                        c => c.RoomID == contract.RoomID && c.ContractID != contract.ContractID && c.Status == "Active");
                    if (other != null)
                        throw new BadRequestException("Phòng đã được thuê bởi hợp đồng khác.");
                }

                contract.TenantID = request.TenantID;
                contract.Status = "Active";
                contract.UpdatedDate = DateTime.Now;
                _unitOfWork.Contracts.Update(contract);

                contract.Room.Status = "Occupied";
                contract.Room.UpdatedDate = DateTime.Now;
                _unitOfWork.Rooms.Update(contract.Room);

                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    UserID = request.TenantID,
                    Title = "Hợp đồng thuê mới",
                    Content = $"Bạn đã được gắn vào hợp đồng {contract.ContractCode} cho phòng {contract.Room.RoomNumber}.",
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });

                await _unitOfWork.ActivityLogs.AddAsync(new ActivityLog
                {
                    UserID = actorUserId,
                    Action = "AssignTenant",
                    Details = $"Gán khách #{request.TenantID} vào {contract.ContractCode}",
                    CreatedDate = DateTime.Now
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
                if (contract.Status != "Active" && contract.Status != "Draft")
                    throw new BadRequestException("Chỉ có thể kết thúc hợp đồng nháp hoặc đang có hiệu lực.");
                contract.Status = "Terminated";
                contract.MoveOutDate = DateTime.Now;
                contract.UpdatedDate = DateTime.Now;
                _unitOfWork.Contracts.Update(contract);

                if (contract.Room != null && contract.Room.Status == "Occupied")
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
            if (contract.Status != "Active" || !contract.TenantID.HasValue)
                throw new BadRequestException("Chỉ gia hạn hợp đồng Active đã có khách thuê.");
            if (newEndDate.Date <= contract.EndDate.Date)
                throw new BadRequestException("Ngày kết thúc mới phải sau ngày kết thúc hiện tại.");

            var oldEnd = contract.EndDate;
            contract.EndDate = newEndDate.Date;
            contract.UpdatedDate = DateTime.Now;
            _unitOfWork.Contracts.Update(contract);

            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserID = contract.TenantID.Value,
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

        public async Task<ContractDto> UpdateContractAsync(UpdateContractDto request, int landlordId)
        {
            var contract = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                c => c.ContractID == request.ContractID, "Room.House, Tenant");
            if (contract == null) throw new NotFoundException("Hợp đồng", request.ContractID);
            if (contract.Room?.House?.OwnerID != landlordId)
                throw new BadRequestException("Bạn không có quyền sửa hợp đồng này.");
            if (contract.Status != "Draft" && contract.Status != "Active")
                throw new BadRequestException("Chỉ sửa hợp đồng nháp hoặc đang hiệu lực.");
            if (request.EndDate.Date <= contract.StartDate.Date)
                throw new BadRequestException("Ngày kết thúc phải sau ngày bắt đầu.");
            if (request.MonthlyRent <= 0)
                throw new BadRequestException("Tiền thuê phải lớn hơn 0.");
            if (request.Deposit < 0 || request.ElectricPrice < 0 || request.WaterPrice < 0)
                throw new BadRequestException("Số tiền không hợp lệ.");
            if (string.Equals(contract.PendingEditStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("Đang có đề xuất sửa chờ khách xác nhận. Hủy đề xuất cũ trước khi gửi mới.");

            bool hasTenant = contract.TenantID.HasValue && contract.Status == "Active";

            if (!hasTenant)
            {
                // Nháp / chưa có khách — sửa ngay
                ApplyLiveValues(contract, request);
                contract.UpdatedDate = DateTime.Now;
                ClearPending(contract);
                _unitOfWork.Contracts.Update(contract);
                await _unitOfWork.SaveChangesAsync();
                return _mapper.Map<ContractDto>(
                    await _unitOfWork.Contracts.FirstOrDefaultAsync(c => c.ContractID == contract.ContractID, "Room, Tenant"));
            }

            // Có khách — chờ xác nhận
            contract.PendingMonthlyRent = request.MonthlyRent;
            contract.PendingElectricPrice = request.ElectricPrice;
            contract.PendingWaterPrice = request.WaterPrice;
            contract.PendingDeposit = request.Deposit;
            contract.PendingEndDate = request.EndDate.Date;
            contract.PendingEditStatus = "Pending";
            contract.PendingEditNote = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
            contract.PendingEditAt = DateTime.Now;
            contract.UpdatedDate = DateTime.Now;
            _unitOfWork.Contracts.Update(contract);

            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserID = contract.TenantID!.Value,
                Title = "Yêu cầu sửa hợp đồng",
                Content = $"Chủ nhà đề xuất sửa {contract.ContractCode}: thuê {request.MonthlyRent:N0}đ, điện {request.ElectricPrice:N0}, nước {request.WaterPrice:N0}, hết hạn {request.EndDate:dd/MM/yyyy}. Vui lòng xác nhận hoặc từ chối.",
                IsRead = false,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            });

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ContractDto>(
                await _unitOfWork.Contracts.FirstOrDefaultAsync(c => c.ContractID == contract.ContractID, "Room, Tenant"));
        }

        public async Task<bool> ConfirmContractEditAsync(int contractId, int tenantId)
        {
            var contract = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                c => c.ContractID == contractId, "Room.House, Tenant");
            if (contract == null) throw new NotFoundException("Hợp đồng", contractId);
            if (contract.TenantID != tenantId)
                throw new BadRequestException("Bạn không phải khách thuê của hợp đồng này.");
            if (!string.Equals(contract.PendingEditStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("Không có đề xuất sửa đang chờ xác nhận.");

            // Lưu giá cũ trước khi áp dụng
            contract.PreviousMonthlyRent = contract.MonthlyRent;
            contract.PreviousElectricPrice = contract.ElectricPrice;
            contract.PreviousWaterPrice = contract.WaterPrice;
            contract.PriceEffectiveDate = DateTime.Now;

            if (contract.PendingMonthlyRent.HasValue) contract.MonthlyRent = contract.PendingMonthlyRent.Value;
            if (contract.PendingElectricPrice.HasValue) contract.ElectricPrice = contract.PendingElectricPrice.Value;
            if (contract.PendingWaterPrice.HasValue) contract.WaterPrice = contract.PendingWaterPrice.Value;
            if (contract.PendingDeposit.HasValue) contract.Deposit = contract.PendingDeposit.Value;
            if (contract.PendingEndDate.HasValue) contract.EndDate = contract.PendingEndDate.Value.Date;

            ClearPending(contract);
            contract.UpdatedDate = DateTime.Now;
            _unitOfWork.Contracts.Update(contract);

            int landlordId = contract.Room.House.OwnerID;
            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserID = landlordId,
                Title = "Khách đã xác nhận sửa HĐ",
                Content = $"Khách thuê đã xác nhận thay đổi hợp đồng {contract.ContractCode}. Giá mới áp dụng từ {contract.PriceEffectiveDate:dd/MM/yyyy HH:mm}.",
                IsRead = false,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            });

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectContractEditAsync(int contractId, int tenantId)
        {
            var contract = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                c => c.ContractID == contractId, "Room.House");
            if (contract == null) throw new NotFoundException("Hợp đồng", contractId);
            if (contract.TenantID != tenantId)
                throw new BadRequestException("Bạn không phải khách thuê của hợp đồng này.");
            if (!string.Equals(contract.PendingEditStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("Không có đề xuất sửa đang chờ xác nhận.");

            ClearPending(contract);
            contract.UpdatedDate = DateTime.Now;
            _unitOfWork.Contracts.Update(contract);

            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserID = contract.Room.House.OwnerID,
                Title = "Khách từ chối sửa HĐ",
                Content = $"Khách thuê đã từ chối đề xuất sửa hợp đồng {contract.ContractCode}.",
                IsRead = false,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            });

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelPendingContractEditAsync(int contractId, int landlordId)
        {
            var contract = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                c => c.ContractID == contractId, "Room.House, Tenant");
            if (contract == null) throw new NotFoundException("Hợp đồng", contractId);
            if (contract.Room?.House?.OwnerID != landlordId)
                throw new BadRequestException("Bạn không có quyền hủy đề xuất này.");
            if (!string.Equals(contract.PendingEditStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("Không có đề xuất đang chờ.");

            ClearPending(contract);
            contract.UpdatedDate = DateTime.Now;
            _unitOfWork.Contracts.Update(contract);

            if (contract.TenantID.HasValue)
            {
                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    UserID = contract.TenantID.Value,
                    Title = "Chủ nhà hủy đề xuất sửa HĐ",
                    Content = $"Đề xuất sửa hợp đồng {contract.ContractCode} đã được chủ nhà hủy.",
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private static void ApplyLiveValues(Contract contract, UpdateContractDto request)
        {
            contract.EndDate = request.EndDate.Date;
            contract.Deposit = request.Deposit;
            contract.MonthlyRent = request.MonthlyRent;
            contract.ElectricPrice = request.ElectricPrice;
            contract.WaterPrice = request.WaterPrice;
        }

        private static void ClearPending(Contract contract)
        {
            contract.PendingMonthlyRent = null;
            contract.PendingElectricPrice = null;
            contract.PendingWaterPrice = null;
            contract.PendingDeposit = null;
            contract.PendingEndDate = null;
            contract.PendingEditStatus = null;
            contract.PendingEditNote = null;
            contract.PendingEditAt = null;
        }
    }
}
