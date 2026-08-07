namespace RPMS.Common.Constants
{
    /// <summary>Loại thông báo có hành động (duyệt/từ chối) trong Notification Center.</summary>
    public static class NotificationActions
    {
        public const string ContractEdit = "ContractEdit";
        public const string ContractCancel = "ContractCancel";
        /// <summary>Đề nghị thuê mới — khách Đồng ý / Từ chối (PendingConfirm).</summary>
        public const string ContractConfirm = "ContractConfirm";

        public const string Pending = "Pending";
        public const string Completed = "Completed";
        public const string Declined = "Declined";
    }
}
