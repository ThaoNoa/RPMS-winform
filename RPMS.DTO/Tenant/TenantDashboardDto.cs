using System.Collections.Generic;
using RPMS.DTO.Contract;
using RPMS.DTO.Invoice;
using RPMS.DTO.Maintenance;
using RPMS.DTO.Notification;
using RPMS.DTO.Tenant;

namespace RPMS.DTO.Tenant
{
    public class TenantDashboardDto
    {
        public ContractDto? CurrentContract { get; set; }
        public List<InvoiceDto> UnpaidInvoices { get; set; } = new();
        public List<MaintenanceRequestDto> RecentMaintenances { get; set; } = new();
        public List<AppointmentDto> UpcomingAppointments { get; set; } = new();
        public List<NotificationDto> RecentNotifications { get; set; } = new();
        public int FavoriteCount { get; set; }
    }
}
