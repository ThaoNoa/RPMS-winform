using System;

namespace RPMS.DTO.Tenant
{
    public class AppointmentDto
    {
        public int AppointmentID { get; set; }
        public int RoomID { get; set; }
        public int TenantID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Note { get; set; } = "";
        public string Status { get; set; } = "";
        public string RoomNumber { get; set; } = "";
        public string TenantName { get; set; } = "";
    }
}