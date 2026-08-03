using System;

namespace RPMS.DTO.ActivityLog
{
    public class ActivityLogDto
    {
        public int LogID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = "";
        public string Action { get; set; } = "";
        public string? Details { get; set; }
        public string? IPAddress { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
