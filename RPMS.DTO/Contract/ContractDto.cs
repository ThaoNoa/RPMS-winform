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
    }
}