using System;

namespace RPMS.DTO.Tenant
{
    public class CreateAppointmentDto
    {
        public int RoomID { get; set; }
        public int TenantID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Note { get; set; } = "";
    }
}