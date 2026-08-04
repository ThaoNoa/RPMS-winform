using RPMS.DTO.Tenant;
using RPMS.DTO.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface ILandlordService
    {
        Task<IEnumerable<AppointmentDto>> GetAppointmentsAsync(int landlordId, int? houseId, string status, DateTime? fromDate, DateTime? toDate);
        Task<bool> UpdateAppointmentStatusAsync(int appointmentId, string status);
        Task<bool> CreateNotificationForTenantsAsync(int landlordId, int? houseId, string title, string content);
        /// <summary>Khách đã đặt lịch xem phòng của landlord (Active). Có thể lọc theo RoomID.</summary>
        Task<IEnumerable<UserDto>> GetAppointmentTenantsAsync(int landlordId, int? roomId = null);
    }
}