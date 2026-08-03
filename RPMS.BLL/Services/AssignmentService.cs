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

        public async Task<AssignmentDto> CreateAsync(CreateAssignmentDto request)
        {
            var house = await _unitOfWork.Houses.GetByIdAsync(request.HouseID);
            if (house == null) throw new NotFoundException("Nhà", request.HouseID);

            var manager = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.UserID == request.ManagerID, "Role");
            if (manager == null) throw new NotFoundException("Quản lý", request.ManagerID);
            if (manager.Role?.RoleName != "Manager" && manager.RoleID != 4)
                throw new BadRequestException("Người được gán phải có vai trò Manager.");

            var exists = await _unitOfWork.Assignments.FirstOrDefaultAsync(
                a => a.HouseID == request.HouseID && a.ManagerID == request.ManagerID && a.Status == "Active");
            if (exists != null)
                throw new BadRequestException("Manager này đã được gán cho nhà này.");

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

        public async Task<bool> DeactivateAsync(int assignmentId)
        {
            var item = await _unitOfWork.Assignments.GetByIdAsync(assignmentId);
            if (item == null) throw new NotFoundException("Phân công", assignmentId);
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
