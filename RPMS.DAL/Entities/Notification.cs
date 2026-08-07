using System;

namespace RPMS.DAL.Entities
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public int UserID { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public bool IsRead { get; set; }
        /// <summary>ContractEdit | ContractCancel | ContractConfirm | null</summary>
        public string? ActionType { get; set; }
        public int? RelatedID { get; set; }
        /// <summary>Pending | Completed | Declined | null</summary>
        public string? ActionStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public virtual User User { get; set; } = null!;
    }
}