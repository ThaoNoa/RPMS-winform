using System;

namespace RPMS.DTO.Contract
{
    public class ContractDto
    {
        public int ContractID { get; set; }
        public string ContractCode { get; set; } = "";
        public int HouseID { get; set; }
        public string HouseName { get; set; } = "";
        public string RoomNumber { get; set; } = "";
        public int? TenantID { get; set; }
        public string TenantName { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public string Status { get; set; } = "";
        /// <summary>Pending khi chủ sửa và chờ khách xác nhận.</summary>
        public string? PendingEditStatus { get; set; }
        /// <summary>Pending khi khách xin hủy thuê, chờ chủ duyệt.</summary>
        public string? CancelRequestStatus { get; set; }
        /// <summary>Tenant | Landlord — ai gửi yêu cầu hủy.</summary>
        public string? CancelRequestedBy { get; set; }
        public string? CancelRequestNote { get; set; }
        /// <summary>Hiển thị lưới: "Khách xin" / "Chủ xin" / rỗng.</summary>
        public string CancelRequestLabel { get; set; } = "";
    }
}