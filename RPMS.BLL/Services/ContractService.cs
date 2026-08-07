using AutoMapper;
using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
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
            var assignments = await _unitOfWork.Assignments.FindAsync(
                a => a.ManagerID == managerId && a.Status == "Active");
            var houseIds = assignments.Select(a => a.HouseID).Distinct().ToList();
            if (houseIds.Count == 0)
                return Array.Empty<ContractDto>();

            // Lấy RoomID trước (tránh filter navigation Room.HouseID bị miss / dịch SQL kém)
            var roomIds = (await _unitOfWork.Rooms.FindAsync(r => houseIds.Contains(r.HouseID)))
                .Select(r => r.RoomID)
                .ToList();
            if (roomIds.Count == 0)
                return Array.Empty<ContractDto>();

            var contracts = await _unitOfWork.Contracts.FindAsync(
                c => roomIds.Contains(c.RoomID),
                "Room.House, Tenant");
            return _mapper.Map<IEnumerable<ContractDto>>(
                contracts.OrderBy(c => c.Room?.House?.HouseName)
                    .ThenBy(c => c.Room?.RoomNumber)
                    .ThenBy(c => c.ContractCode));
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
                    c => c.RoomID == request.RoomID
                         && (c.Status == "Active" || c.Status == "Draft" || c.Status == "PendingConfirm"));
                if (openContract != null)
                    throw new BadRequestException("Phòng này đã có hợp đồng nháp, chờ xác nhận hoặc đang hiệu lực.");

                int? tenantId = request.TenantID is > 0 ? request.TenantID : null;
                if (tenantId.HasValue)
                {
                    var tenant = await _unitOfWork.Users.GetByIdAsync(tenantId.Value);
                    if (tenant == null || tenant.Status != "Active")
                        throw new BadRequestException("Khách thuê không hợp lệ hoặc không còn hoạt động.");
                }

                bool hasTenant = tenantId.HasValue;
                // Có khách → chờ khách xác nhận; chưa Occupied cho đến khi Accept
                var contract = new Contract
                {
                    ContractCode = $"HD{DateTime.Now:yyyyMMddHHmmss}{request.RoomID:D4}",
                    RoomID = request.RoomID,
                    TenantID = tenantId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    MoveInDate = request.StartDate,
                    Deposit = request.Deposit,
                    MonthlyRent = request.MonthlyRent,
                    ElectricPrice = request.ElectricPrice,
                    WaterPrice = request.WaterPrice,
                    Status = hasTenant ? "PendingConfirm" : "Draft",
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
                        Title = "Đề nghị thuê phòng — cần xác nhận",
                        Content = $"Chủ nhà mời bạn thuê phòng {room.RoomNumber} (HĐ {contract.ContractCode}).\n\nMở thông báo này → Xem chi tiết để Đồng ý hoặc Từ chối, hoặc vào «Hợp đồng» bấm Đồng ý / Từ chối.",
                        ActionType = NotificationActions.ContractConfirm,
                        RelatedID = contract.ContractID,
                        ActionStatus = NotificationActions.Pending,
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

        public async Task<BulkCreateDraftContractsResultDto> CreateDraftContractsForHouseAsync(
            BulkCreateDraftContractsDto request, int landlordId)
        {
            if (request.EndDate <= request.StartDate)
                throw new BadRequestException("Ngày kết thúc phải lớn hơn ngày bắt đầu.");
            if (request.ElectricPrice < 0 || request.WaterPrice < 0 || request.Deposit < 0)
                throw new BadRequestException("Giá điện/nước/cọc không hợp lệ.");

            var house = await _unitOfWork.Houses.GetByIdAsync(request.HouseID);
            if (house == null) throw new NotFoundException("Nhà", request.HouseID);
            if (house.OwnerID != landlordId)
                throw new BadRequestException("Bạn chỉ được tạo hợp đồng cho nhà của mình.");

            var rooms = (await _unitOfWork.Rooms.FindAsync(r => r.HouseID == request.HouseID)).ToList();
            if (rooms.Count == 0)
                return new BulkCreateDraftContractsResultDto
                {
                    CreatedCount = 0,
                    SkippedCount = 0,
                    Message = "Nhà này chưa có phòng."
                };

            var openRoomIds = (await _unitOfWork.Contracts.FindAsync(
                    c => c.Room.HouseID == request.HouseID
                         && (c.Status == "Active" || c.Status == "Draft" || c.Status == "PendingConfirm")))
                .Select(c => c.RoomID)
                .ToHashSet();

            var eligible = rooms
                .Where(r => !openRoomIds.Contains(r.RoomID)
                            && !string.Equals(r.Status, "Occupied", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(r.Status, "Inactive", StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => r.RoomNumber)
                .ToList();

            if (eligible.Count == 0)
            {
                return new BulkCreateDraftContractsResultDto
                {
                    CreatedCount = 0,
                    SkippedCount = rooms.Count,
                    Message = "Không còn phòng nào chưa có hợp đồng (nháp/hiệu lực) để tạo."
                };
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                int seq = 0;
                foreach (var room in eligible)
                {
                    seq++;
                    decimal rent = request.MonthlyRent > 0 ? request.MonthlyRent : room.Price;
                    if (rent <= 0)
                        throw new BadRequestException($"Phòng {room.RoomNumber} chưa có giá thuê — nhập tiền thuê trên form hoặc cập nhật giá phòng.");

                    var contract = new Contract
                    {
                        ContractCode = $"HD{DateTime.Now:yyyyMMddHHmmss}{room.RoomID:D4}{seq:D2}",
                        RoomID = room.RoomID,
                        TenantID = null,
                        StartDate = request.StartDate.Date,
                        EndDate = request.EndDate.Date,
                        MoveInDate = request.StartDate.Date,
                        Deposit = request.Deposit,
                        MonthlyRent = rent,
                        ElectricPrice = request.ElectricPrice,
                        WaterPrice = request.WaterPrice,
                        Status = "Draft",
                        CreatedBy = landlordId,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };
                    await _unitOfWork.Contracts.AddAsync(contract);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                int skipped = rooms.Count - eligible.Count;
                return new BulkCreateDraftContractsResultDto
                {
                    CreatedCount = eligible.Count,
                    SkippedCount = skipped,
                    Message = $"Đã tạo {eligible.Count} hợp đồng nháp" +
                              (skipped > 0 ? $" (bỏ qua {skipped} phòng đã có HĐ hoặc Occupied)." : ".")
                };
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
                if (contract.TenantID.HasValue && contract.Status != "Draft")
                    throw new BadRequestException("Hợp đồng đã có khách thuê hoặc đang chờ xác nhận.");
                if (contract.Status != "Draft")
                    throw new BadRequestException("Chỉ gán khách cho hợp đồng nháp (Draft).");

                var tenant = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.UserID == request.TenantID);
                if (tenant == null || !string.Equals(tenant.Status, "Active", StringComparison.OrdinalIgnoreCase))
                    throw new BadRequestException("Khách thuê không hợp lệ hoặc không còn hoạt động.");
                if (tenant.RoleID != 3)
                    throw new BadRequestException("Người dùng được gán phải có vai trò Tenant.");

                if (contract.Room == null)
                    throw new NotFoundException("Phòng", contract.RoomID);
                if (contract.Room.Status == "Occupied")
                {
                    var other = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                        c => c.RoomID == contract.RoomID && c.ContractID != contract.ContractID && c.Status == "Active");
                    if (other != null)
                        throw new BadRequestException("Phòng đã được thuê bởi hợp đồng khác.");
                }

                var otherPending = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                    c => c.RoomID == contract.RoomID
                         && c.ContractID != contract.ContractID
                         && c.Status == "PendingConfirm");
                if (otherPending != null)
                    throw new BadRequestException("Phòng đang có đề nghị thuê chờ khách khác xác nhận.");

                // Chờ khách xác nhận — chưa Occupied / chưa Active
                contract.TenantID = request.TenantID;
                contract.Status = "PendingConfirm";
                contract.UpdatedDate = DateTime.Now;
                _unitOfWork.Contracts.Update(contract);

                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    UserID = request.TenantID,
                    Title = "Đề nghị thuê phòng — cần xác nhận",
                    Content = $"Chủ nhà mời bạn thuê phòng {contract.Room.RoomNumber} (HĐ {contract.ContractCode}).\n\nMở thông báo này → Xem chi tiết để Đồng ý hoặc Từ chối, hoặc vào «Hợp đồng» bấm Đồng ý / Từ chối.",
                    ActionType = NotificationActions.ContractConfirm,
                    RelatedID = contract.ContractID,
                    ActionStatus = NotificationActions.Pending,
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });

                await _unitOfWork.ActivityLogs.AddAsync(new ActivityLog
                {
                    UserID = actorUserId,
                    Action = "AssignTenant",
                    Details = $"Gửi đề nghị thuê cho khách #{request.TenantID} — {contract.ContractCode} (PendingConfirm)",
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

        public async Task<bool> AcceptRentalOfferAsync(int contractId, int tenantId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var contract = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                    c => c.ContractID == contractId, "Room, Room.House, Tenant");
                if (contract == null) throw new NotFoundException("Hợp đồng", contractId);
                if (!string.Equals(contract.Status, "PendingConfirm", StringComparison.OrdinalIgnoreCase))
                    throw new BadRequestException("Hợp đồng không đang chờ xác nhận thuê.");
                if (contract.TenantID != tenantId)
                    throw new BadRequestException("Bạn không phải khách được mời trên hợp đồng này.");
                if (contract.Room == null)
                    throw new NotFoundException("Phòng", contract.RoomID);
                if (contract.Room.Status == "Occupied")
                {
                    var other = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                        c => c.RoomID == contract.RoomID && c.ContractID != contract.ContractID && c.Status == "Active");
                    if (other != null)
                        throw new BadRequestException("Phòng đã được thuê. Không thể xác nhận.");
                }

                contract.Status = "Active";
                contract.MoveInDate = DateTime.Today;
                contract.UpdatedDate = DateTime.Now;
                _unitOfWork.Contracts.Update(contract);

                contract.Room.Status = "Occupied";
                contract.Room.UpdatedDate = DateTime.Now;
                _unitOfWork.Rooms.Update(contract.Room);

                await CompletePendingActionNotificationsAsync(
                    NotificationActions.ContractConfirm, contractId, NotificationActions.Completed, save: false);

                int? landlordId = contract.Room.House?.OwnerID;
                if (landlordId is > 0)
                {
                    await _unitOfWork.Notifications.AddAsync(new Notification
                    {
                        UserID = landlordId.Value,
                        Title = "Khách đã đồng ý thuê",
                        Content = $"Khách đã xác nhận thuê phòng {contract.Room.RoomNumber} (HĐ {contract.ContractCode}). Phòng đã Occupied — Dashboard «Đã thuê» cập nhật.",
                        IsRead = false,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    });
                }

                await _unitOfWork.ActivityLogs.AddAsync(new ActivityLog
                {
                    UserID = tenantId,
                    Action = "AcceptRental",
                    Details = $"Khách đồng ý thuê {contract.ContractCode}",
                    CreatedDate = DateTime.Now
                });

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

        public async Task<bool> RejectRentalOfferAsync(int contractId, int tenantId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var contract = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                    c => c.ContractID == contractId, "Room, Room.House, Tenant");
                if (contract == null) throw new NotFoundException("Hợp đồng", contractId);
                if (!string.Equals(contract.Status, "PendingConfirm", StringComparison.OrdinalIgnoreCase))
                    throw new BadRequestException("Hợp đồng không đang chờ xác nhận thuê.");
                if (contract.TenantID != tenantId)
                    throw new BadRequestException("Bạn không phải khách được mời trên hợp đồng này.");

                string roomNo = contract.Room?.RoomNumber ?? "";
                string code = contract.ContractCode;
                int? landlordId = contract.Room?.House?.OwnerID;

                contract.TenantID = null;
                contract.Status = "Draft";
                contract.UpdatedDate = DateTime.Now;
                _unitOfWork.Contracts.Update(contract);

                await CompletePendingActionNotificationsAsync(
                    NotificationActions.ContractConfirm, contractId, NotificationActions.Declined, save: false);

                if (landlordId is > 0)
                {
                    await _unitOfWork.Notifications.AddAsync(new Notification
                    {
                        UserID = landlordId.Value,
                        Title = "Khách từ chối thuê",
                        Content = $"Khách đã từ chối đề nghị thuê phòng {roomNo} (HĐ {code}). Hợp đồng trở lại nháp.",
                        IsRead = false,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    });
                }

                await _unitOfWork.ActivityLogs.AddAsync(new ActivityLog
                {
                    UserID = tenantId,
                    Action = "RejectRental",
                    Details = $"Khách từ chối thuê {code}",
                    CreatedDate = DateTime.Now
                });

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

        public async Task<bool> TerminateContractAsync(int contractId)
            => await TerminateContractAsync(contractId, 0, null);

        public async Task<bool> TerminateContractAsync(int contractId, int actorUserId, string? reason = null)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var contract = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                    c => c.ContractID == contractId, "Room, Room.House, Tenant");
                if (contract == null) throw new NotFoundException("Hợp đồng", contractId);
                if (contract.Status != "Active" && contract.Status != "Draft" && contract.Status != "PendingConfirm")
                    throw new BadRequestException("Chỉ có thể kết thúc hợp đồng nháp, chờ xác nhận hoặc đang hiệu lực.");

                string prevStatus = contract.Status;
                bool freeRoom = string.Equals(prevStatus, "Active", StringComparison.OrdinalIgnoreCase)
                    || (contract.Room != null && contract.Room.Status == "Occupied");

                // CHECK MoveOutDate >= MoveInDate — nếu hủy trước ngày nhận phòng thì gán MoveOut = MoveIn
                var moveIn = contract.MoveInDate == default ? contract.StartDate.Date : contract.MoveInDate.Date;
                var moveOut = DateTime.Now;
                if (moveOut.Date < moveIn)
                    moveOut = moveIn;

                contract.Status = "Terminated";
                contract.MoveOutDate = moveOut;
                contract.UpdatedDate = DateTime.Now;
                ClearPending(contract);
                ClearCancelRequest(contract);
                _unitOfWork.Contracts.Update(contract);

                await CompletePendingActionNotificationsAsync(
                    NotificationActions.ContractCancel, contractId, NotificationActions.Completed, save: false);
                await CompletePendingActionNotificationsAsync(
                    NotificationActions.ContractEdit, contractId, NotificationActions.Declined, save: false);
                await CompletePendingActionNotificationsAsync(
                    NotificationActions.ContractConfirm, contractId, NotificationActions.Declined, save: false);

                if (freeRoom && contract.Room != null)
                {
                    contract.Room.Status = "Available";
                    contract.Room.UpdatedDate = DateTime.Now;
                    _unitOfWork.Rooms.Update(contract.Room);
                }

                string reasonText = string.IsNullOrWhiteSpace(reason) ? "" : $" Lý do: {reason.Trim()}";
                string roomNo = contract.Room?.RoomNumber ?? "?";

                if (contract.TenantID is > 0)
                {
                    string title = prevStatus == "PendingConfirm"
                        ? "Chủ nhà đã hủy đề nghị thuê"
                        : "Hợp đồng đã kết thúc";
                    string content = prevStatus == "PendingConfirm"
                        ? $"Đề nghị thuê phòng {roomNo} (HĐ {contract.ContractCode}) đã bị chủ nhà hủy.{reasonText}"
                        : $"Hợp đồng {contract.ContractCode} (phòng {roomNo}) đã được kết thúc.{reasonText} Phòng trở lại trống.";
                    await _unitOfWork.Notifications.AddAsync(new Notification
                    {
                        UserID = contract.TenantID.Value,
                        Title = title,
                        Content = content,
                        IsRead = false,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    });
                }

                int? landlordId = contract.Room?.House?.OwnerID;
                if (landlordId is > 0 && actorUserId > 0 && actorUserId != landlordId.Value
                    && contract.TenantID == actorUserId)
                {
                    // Trường hợp hiếm: tenant gọi terminate trực tiếp — vẫn báo chủ
                    await _unitOfWork.Notifications.AddAsync(new Notification
                    {
                        UserID = landlordId.Value,
                        Title = "Hợp đồng đã kết thúc",
                        Content = $"HĐ {contract.ContractCode} (phòng {roomNo}) đã kết thúc.{reasonText}",
                        IsRead = false,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    });
                }

                if (actorUserId > 0)
                {
                    await _unitOfWork.ActivityLogs.AddAsync(new ActivityLog
                    {
                        UserID = actorUserId,
                        Action = "TerminateContract",
                        Details = $"Hủy {contract.ContractCode} ({prevStatus}→Terminated).{reasonText}",
                        CreatedDate = DateTime.Now
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

        public async Task<bool> RequestCancelAsync(int contractId, int actorUserId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new BadRequestException("Vui lòng nhập lý do xin hủy thuê.");

            var contract = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                c => c.ContractID == contractId, "Room.House, Tenant");
            if (contract == null) throw new NotFoundException("Hợp đồng", contractId);
            if (!string.Equals(contract.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("Chỉ xin hủy khi hợp đồng đang Active.");
            if (!contract.TenantID.HasValue)
                throw new BadRequestException("Hợp đồng chưa có khách thuê.");

            int landlordId = contract.Room?.House?.OwnerID ?? 0;
            bool isTenant = contract.TenantID == actorUserId;
            bool isLandlord = landlordId > 0 && landlordId == actorUserId;
            if (!isTenant && !isLandlord)
                throw new BadRequestException("Bạn không có quyền xin hủy hợp đồng này.");

            if (string.Equals(contract.CancelRequestStatus, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                string who = string.Equals(contract.CancelRequestedBy, "Landlord", StringComparison.OrdinalIgnoreCase)
                    ? "chủ nhà" : "khách thuê";
                throw new BadRequestException($"Đã có yêu cầu hủy từ {who} đang chờ phản hồi.");
            }

            string by = isLandlord ? "Landlord" : "Tenant";
            contract.CancelRequestStatus = "Pending";
            contract.CancelRequestedBy = by;
            contract.CancelRequestNote = reason.Trim();
            contract.CancelRequestAt = DateTime.Now;
            contract.UpdatedDate = DateTime.Now;
            _unitOfWork.Contracts.Update(contract);

            string roomNo = contract.Room?.RoomNumber ?? "?";
            string tenantName = contract.Tenant?.FullName ?? "Khách thuê";
            if (isTenant && landlordId > 0)
            {
                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    UserID = landlordId,
                    Title = $"Khách xin hủy thuê — {contract.ContractCode}",
                    Content = $"Khách {tenantName} (phòng {roomNo}) xin hủy thuê.\nChi tiết: {reason.Trim()}\n\nMở thông báo này → Xem chi tiết để Duyệt hủy hoặc Từ chối hủy.",
                    ActionType = NotificationActions.ContractCancel,
                    RelatedID = contract.ContractID,
                    ActionStatus = NotificationActions.Pending,
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });
            }
            else if (isLandlord)
            {
                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    UserID = contract.TenantID.Value,
                    Title = $"Chủ nhà xin hủy thuê — {contract.ContractCode}",
                    Content = $"Chủ nhà đề nghị kết thúc HĐ phòng {roomNo}.\nChi tiết: {reason.Trim()}\n\nMở thông báo này → Xem chi tiết để Duyệt hủy hoặc Từ chối hủy.",
                    ActionType = NotificationActions.ContractCancel,
                    RelatedID = contract.ContractID,
                    ActionStatus = NotificationActions.Pending,
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });
            }

            await _unitOfWork.ActivityLogs.AddAsync(new ActivityLog
            {
                UserID = actorUserId,
                Action = "RequestCancel",
                Details = $"{by} xin hủy {contract.ContractCode}: {reason.Trim()}",
                CreatedDate = DateTime.Now
            });

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveCancelRequestAsync(int contractId, int actorUserId)
        {
            var contract = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                c => c.ContractID == contractId, "Room.House, Tenant");
            if (contract == null) throw new NotFoundException("Hợp đồng", contractId);
            if (!string.Equals(contract.CancelRequestStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("Không có yêu cầu hủy đang chờ.");

            int landlordId = contract.Room?.House?.OwnerID ?? 0;
            string by = contract.CancelRequestedBy ?? "";
            // Người duyệt phải là bên kia
            if (string.Equals(by, "Tenant", StringComparison.OrdinalIgnoreCase))
            {
                if (landlordId != actorUserId)
                    throw new BadRequestException("Chỉ chủ nhà được duyệt yêu cầu hủy của khách.");
            }
            else if (string.Equals(by, "Landlord", StringComparison.OrdinalIgnoreCase))
            {
                if (contract.TenantID != actorUserId)
                    throw new BadRequestException("Chỉ khách thuê được duyệt yêu cầu hủy của chủ nhà.");
            }
            else
                throw new BadRequestException("Yêu cầu hủy không hợp lệ.");

            string reason = string.IsNullOrWhiteSpace(contract.CancelRequestNote)
                ? $"Duyệt yêu cầu hủy từ {(by == "Landlord" ? "chủ nhà" : "khách")}"
                : $"Duyệt hủy: {contract.CancelRequestNote}";

            bool ok = await TerminateContractAsync(contractId, actorUserId, reason);
            await CompletePendingActionNotificationsAsync(
                NotificationActions.ContractCancel, contractId, NotificationActions.Completed);
            return ok;
        }

        public async Task<bool> RejectCancelRequestAsync(int contractId, int actorUserId, string? note = null)
        {
            var contract = await _unitOfWork.Contracts.FirstOrDefaultAsync(
                c => c.ContractID == contractId, "Room.House, Tenant");
            if (contract == null) throw new NotFoundException("Hợp đồng", contractId);
            if (!string.Equals(contract.CancelRequestStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("Hợp đồng không có yêu cầu hủy đang chờ.");

            int landlordId = contract.Room?.House?.OwnerID ?? 0;
            string by = contract.CancelRequestedBy ?? "";
            int notifyUserId;
            string rejectorLabel;

            if (string.Equals(by, "Tenant", StringComparison.OrdinalIgnoreCase))
            {
                if (landlordId != actorUserId)
                    throw new BadRequestException("Chỉ chủ nhà được từ chối yêu cầu hủy của khách.");
                notifyUserId = contract.TenantID ?? 0;
                rejectorLabel = "Chủ nhà";
            }
            else if (string.Equals(by, "Landlord", StringComparison.OrdinalIgnoreCase))
            {
                if (contract.TenantID != actorUserId)
                    throw new BadRequestException("Chỉ khách thuê được từ chối yêu cầu hủy của chủ nhà.");
                notifyUserId = landlordId;
                rejectorLabel = "Khách thuê";
            }
            else
                throw new BadRequestException("Yêu cầu hủy không hợp lệ.");

            ClearCancelRequest(contract);
            contract.UpdatedDate = DateTime.Now;
            _unitOfWork.Contracts.Update(contract);

            if (notifyUserId > 0)
            {
                string extra = string.IsNullOrWhiteSpace(note) ? "" : $" Phản hồi: {note.Trim()}";
                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    UserID = notifyUserId,
                    Title = "Yêu cầu hủy thuê bị từ chối",
                    Content = $"{rejectorLabel} từ chối yêu cầu hủy HĐ {contract.ContractCode}.{extra} Hợp đồng vẫn Active.",
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });
            }

            await CompletePendingActionNotificationsAsync(
                NotificationActions.ContractCancel, contractId, NotificationActions.Declined, save: false);
            await _unitOfWork.SaveChangesAsync();
            return true;
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
                Content = $"Chủ nhà đề xuất sửa {contract.ContractCode}:\n• Thuê: {request.MonthlyRent:N0}đ\n• Điện: {request.ElectricPrice:N0}\n• Nước: {request.WaterPrice:N0}\n• Hết hạn: {request.EndDate:dd/MM/yyyy}\n{(string.IsNullOrWhiteSpace(request.Note) ? "" : "• Ghi chú: " + request.Note.Trim() + "\n")}\nMở thông báo → Xem chi tiết để Xác nhận hoặc Từ chối.",
                ActionType = NotificationActions.ContractEdit,
                RelatedID = contract.ContractID,
                ActionStatus = NotificationActions.Pending,
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

            await CompletePendingActionNotificationsAsync(
                NotificationActions.ContractEdit, contractId, NotificationActions.Completed, save: false);
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

            await CompletePendingActionNotificationsAsync(
                NotificationActions.ContractEdit, contractId, NotificationActions.Declined, save: false);
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

            await CompletePendingActionNotificationsAsync(
                NotificationActions.ContractEdit, contractId, NotificationActions.Declined, save: false);
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

        private static void ClearCancelRequest(Contract contract)
        {
            contract.CancelRequestStatus = null;
            contract.CancelRequestedBy = null;
            contract.CancelRequestNote = null;
            contract.CancelRequestAt = null;
        }

        private async Task CompletePendingActionNotificationsAsync(
            string actionType, int relatedId, string newStatus, bool save = true)
        {
            var items = await _unitOfWork.Notifications.FindAsync(n =>
                n.RelatedID == relatedId
                && n.ActionType == actionType
                && n.ActionStatus == NotificationActions.Pending);
            foreach (var n in items)
            {
                n.ActionStatus = newStatus;
                n.IsRead = true;
                n.UpdatedDate = DateTime.Now;
                _unitOfWork.Notifications.Update(n);
            }
            if (save && items.Any())
                await _unitOfWork.SaveChangesAsync();
        }
    }
}
