using RPMS.DTO.Tenant;
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
    }
}