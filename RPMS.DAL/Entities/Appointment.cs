using System;

namespace RPMS.DAL.Entities
{
    public class Appointment
    {
        public int AppointmentID { get; set; }
        public int RoomID { get; set; }
        public int TenantID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = "";
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public virtual Room Room { get; set; } = null!;
        public virtual User Tenant { get; set; } = null!;
    }
}