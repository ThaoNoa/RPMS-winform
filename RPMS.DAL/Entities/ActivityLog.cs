using System;

namespace RPMS.DAL.Entities
{
    public class ActivityLog
    {
        public int LogID { get; set; }
        public int UserID { get; set; }
        public string Action { get; set; } = "";
        public string? Details { get; set; }
        public string? IPAddress { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual User User { get; set; } = null!;
    }
}