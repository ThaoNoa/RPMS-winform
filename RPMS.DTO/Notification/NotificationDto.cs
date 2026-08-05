using System;

namespace RPMS.DTO.Notification
{
    public class NotificationDto
    {
        public int NotificationID { get; set; }
        public int UserID { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public bool IsRead { get; set; }
        public string? ActionType { get; set; }
        public int? RelatedID { get; set; }
        public string? ActionStatus { get; set; }
        public bool CanAct =>
            !string.IsNullOrWhiteSpace(ActionType)
            && RelatedID is > 0
            && string.Equals(ActionStatus, "Pending", StringComparison.OrdinalIgnoreCase);
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UserName { get; set; } = "";
    }

    public class CreateNotificationDto
    {
        public int UserID { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string? ActionType { get; set; }
        public int? RelatedID { get; set; }
        public string? ActionStatus { get; set; }
    }
}