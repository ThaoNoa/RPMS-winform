using AutoMapper;
using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.DAL.Entities;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Tenant;
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
                Note = a.Note,
                Status = a.Status
            });
        }

        public async Task<bool> UpdateAppointmentStatusAsync(int appointmentId, string status)
        {
            var app = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (app == null) throw new NotFoundException("Lịch hẹn", appointmentId);

            app.Status = status;
            app.UpdatedDate = DateTime.Now;
            _unitOfWork.Appointments.Update(app);

            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserID = app.TenantID,
                Title = "Cập nhật lịch hẹn",
                Content = $"Lịch hẹn xem phòng của bạn vào {app.AppointmentDate:dd/MM} đã được chuyển sang trạng thái: {status}",
                CreatedDate = DateTime.Now
            });

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateNotificationForTenantsAsync(int landlordId, int? houseId, string title, string content)
        {
            var contracts = await _unitOfWork.Contracts.FindAsync(c => c.Status == "Active" && c.Room.House.OwnerID == landlordId, "Room");
            if (houseId.HasValue && houseId > 0)
                contracts = contracts.Where(c => c.Room.HouseID == houseId);

            var tenantIds = contracts.Select(c => c.TenantID).Distinct().ToList();
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