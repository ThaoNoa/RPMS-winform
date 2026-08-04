using AutoMapper;
using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.DAL.Entities;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Tenant;
using RPMS.DTO.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class LandlordService : ILandlordService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LandlordService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsAsync(int landlordId, int? houseId, string status, DateTime? fromDate, DateTime? toDate)
        {
            var query = await _unitOfWork.Appointments.FindAsync(
                a => a.Room.House.OwnerID == landlordId, "Room.House,Tenant");

            if (houseId.HasValue && houseId.Value > 0)
                query = query.Where(a => a.Room.HouseID == houseId.Value);

            if (!string.IsNullOrEmpty(status) && status != "All")
                query = query.Where(a => a.Status == status);

            if (fromDate.HasValue)
                query = query.Where(a => a.AppointmentDate.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(a => a.AppointmentDate.Date <= toDate.Value.Date);

            return query.OrderByDescending(a => a.AppointmentDate).Select(a => new AppointmentDto
            {
                AppointmentID = a.AppointmentID,
                RoomID = a.RoomID,
                TenantID = a.TenantID,
                AppointmentDate = a.AppointmentDate,
                Note = a.Note ?? "",
                Status = a.Status,
                RoomNumber = a.Room?.RoomNumber ?? "",
                TenantName = a.Tenant?.FullName ?? ""
            });
        }

        public async Task<IEnumerable<UserDto>> GetAppointmentTenantsAsync(int landlordId, int? roomId = null)
        {
            var appointments = await _unitOfWork.Appointments.FindAsync(
                a => a.Room.House.OwnerID == landlordId
                     && a.Status != "Rejected"
                     && a.Status != "Cancelled",
                "Room.House,Tenant");

            if (roomId is > 0)
                appointments = appointments.Where(a => a.RoomID == roomId.Value);

            return appointments
                .Where(a => a.Tenant != null && a.Tenant.Status == "Active")
                .GroupBy(a => a.TenantID)
                .Select(g => g.First().Tenant!)
                .OrderBy(t => t.FullName)
                .Select(t => new UserDto
                {
                    UserID = t.UserID,
                    FullName = t.FullName,
                    Phone = t.Phone ?? "",
                    Email = t.Email ?? "",
                    Username = t.Username,
                    Status = t.Status,
                    RoleID = t.RoleID
                })
                .ToList();
        }

        public async Task<bool> UpdateAppointmentStatusAsync(int appointmentId, string status)
        {
            var app = await _unitOfWork.Appointments.FirstOrDefaultAsync(
                a => a.AppointmentID == appointmentId, "Room.House,Tenant");
            if (app == null) throw new NotFoundException("Lịch hẹn", appointmentId);

            string[] allowed = { "Pending", "Accepted", "Rejected", "Cancelled", "Completed" };
            if (!allowed.Contains(status, StringComparer.OrdinalIgnoreCase))
                throw new BadRequestException("Trạng thái lịch hẹn không hợp lệ.");

            app.Status = status;
            app.UpdatedDate = DateTime.Now;
            _unitOfWork.Appointments.Update(app);

            string statusVi = status switch
            {
                "Accepted" => "Đã xác nhận",
                "Rejected" => "Đã từ chối",
                "Completed" => "Đã hoàn thành",
                "Cancelled" => "Đã hủy",
                "Pending" => "Chờ xác nhận",
                _ => status
            };
            string roomNo = app.Room?.RoomNumber ?? $"#{app.RoomID}";
            string houseName = app.Room?.House?.HouseName ?? "nhà trọ";

            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserID = app.TenantID,
                Title = status == "Accepted"
                    ? "Lịch hẹn đã được xác nhận"
                    : status == "Rejected"
                        ? "Lịch hẹn đã bị từ chối"
                        : "Cập nhật lịch hẹn",
                Content = $"Lịch xem phòng {roomNo} ({houseName}) lúc {app.AppointmentDate:dd/MM/yyyy HH:mm} — trạng thái: {statusVi}."
                    + (string.IsNullOrWhiteSpace(app.Note) ? "" : $" Ghi chú của bạn: {app.Note}"),
                IsRead = false,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            });

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateNotificationForTenantsAsync(int landlordId, int? houseId, string title, string content)
        {
            var contracts = await _unitOfWork.Contracts.FindAsync(c => c.Status == "Active" && c.Room.House.OwnerID == landlordId, "Room");
            if (houseId.HasValue && houseId > 0)
                contracts = contracts.Where(c => c.Room.HouseID == houseId);

            var tenantIds = contracts
                .Where(c => c.TenantID.HasValue)
                .Select(c => c.TenantID!.Value)
                .Distinct()
                .ToList();
            var notifications = tenantIds.Select(tid => new Notification
            {
                UserID = tid,
                Title = title,
                Content = content,
                IsRead = false,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            });

            await _unitOfWork.Notifications.AddRangeAsync(notifications);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}