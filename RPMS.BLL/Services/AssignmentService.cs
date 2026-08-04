using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.DAL.Entities;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Assignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AssignmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<AssignmentDto>> GetAllAsync()
        {
            var items = await _unitOfWork.Assignments.GetAllAsync("House, Manager");
            return items.OrderByDescending(a => a.AssignedDate).Select(Map).ToList();
        }

        public async Task<IEnumerable<AssignmentDto>> GetByLandlordAsync(int landlordId)
        {
            var items = await _unitOfWork.Assignments.FindAsync(
                a => a.House.OwnerID == landlordId,
                "House, Manager");
            return items.OrderByDescending(a => a.AssignedDate).Select(Map).ToList();
        }

        public async Task<IEnumerable<AssignmentDto>> GetByManagerAsync(int managerId)
        {
            var items = await _unitOfWork.Assignments.FindAsync(
                a => a.ManagerID == managerId,
                "House, Manager");
            return items.OrderByDescending(a => a.AssignedDate).Select(Map).ToList();
        }

        public async Task<AssignmentDto> CreateAsync(CreateAssignmentDto request, int landlordId)
        {
            var house = await _unitOfWork.Houses.GetByIdAsync(request.HouseID);
            if (house == null) throw new NotFoundException("Nhà", request.HouseID);
            if (house.OwnerID != landlordId)
                throw new BadRequestException("Bạn chỉ được gán Manager cho nhà của mình.");

            var manager = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.UserID == request.ManagerID, "Role");
            if (manager == null) throw new NotFoundException("Quản lý", request.ManagerID);
            if (manager.Role?.RoleName != "Manager" && manager.RoleID != 4)
                throw new BadRequestException("Người được gán phải có vai trò Manager.");
            if (manager.Status != "Active")
                throw new BadRequestException("Tài khoản Manager không còn hoạt động.");

            // Chỉ gán Manager khi nhà đã có HĐ Active (khách đã Đồng ý thuê)
            bool hasActiveRental = await _unitOfWork.Contracts.ExistsAsync(
                c => c.Room.HouseID == request.HouseID && c.Status == "Active");
            if (!hasActiveRental)
                throw new BadRequestException(
                    "Chỉ gán Manager sau khi khách đã đồng ý thuê (hợp đồng Active). " +
                    "Nhà chưa có phòng đang thuê thì chưa thể phân công.");

            // Unique (HouseID, ManagerID): nếu đã ngưng thì kích hoạt lại, không insert trùng
            var existing = await _unitOfWork.Assignments.FirstOrDefaultAsync(
                a => a.HouseID == request.HouseID && a.ManagerID == request.ManagerID);
            if (existing != null)
            {
                if (existing.Status == "Active")
                    throw new BadRequestException("Manager này đã được gán Active cho nhà này.");

                existing.Status = "Active";
                existing.AssignedDate = DateTime.Now;
                existing.UpdatedDate = DateTime.Now;
                _unitOfWork.Assignments.Update(existing);

                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    UserID = request.ManagerID,
                    Title = "Được gán quản lý nhà",
                    Content = $"Bạn được gán lại quản lý nhà: {house.HouseName} ({house.Address}).",
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });
                await _unitOfWork.SaveChangesAsync();

                var reactivated = await _unitOfWork.Assignments.FirstOrDefaultAsync(
                    a => a.AssignmentID == existing.AssignmentID, "House, Manager");
                return Map(reactivated!);
            }

            var entity = new Assignment
            {
                HouseID = request.HouseID,
                ManagerID = request.ManagerID,
                AssignedDate = DateTime.Now,
                Status = "Active",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            await _unitOfWork.Assignments.AddAsync(entity);

            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserID = request.ManagerID,
                Title = "Được gán quản lý nhà",
                Content = $"Bạn được gán quản lý nhà: {house.HouseName} ({house.Address}).",
                IsRead = false,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            });

            await _unitOfWork.SaveChangesAsync();

            var saved = await _unitOfWork.Assignments.FirstOrDefaultAsync(
                a => a.AssignmentID == entity.AssignmentID, "House, Manager");
            return Map(saved!);
        }

        public async Task<bool> DeactivateAsync(int assignmentId, int landlordId)
        {
            var item = await _unitOfWork.Assignments.FirstOrDefaultAsync(
                a => a.AssignmentID == assignmentId, "House");
            if (item == null) throw new NotFoundException("Phân công", assignmentId);
            if (item.House == null || item.House.OwnerID != landlordId)
                throw new BadRequestException("Bạn chỉ được ngưng phân công nhà của mình.");
            item.Status = "Inactive";
            item.UpdatedDate = DateTime.Now;
            _unitOfWork.Assignments.Update(item);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private static AssignmentDto Map(Assignment a) => new()
        {
            AssignmentID = a.AssignmentID,
            HouseID = a.HouseID,
            HouseName = a.House?.HouseName ?? "",
            HouseAddress = a.House?.Address ?? "",
            ManagerID = a.ManagerID,
            ManagerName = a.Manager?.FullName ?? "",
            AssignedDate = a.AssignedDate,
            Status = a.Status
        };
    }
}
