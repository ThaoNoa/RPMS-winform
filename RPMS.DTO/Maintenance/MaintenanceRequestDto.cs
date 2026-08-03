using System;

namespace RPMS.DTO.Maintenance
{
    public class MaintenanceRequestDto
    {
        public int RequestID { get; set; }
        public int ContractID { get; set; }
        public string ContractCode { get; set; } = "";
        public string RoomNumber { get; set; } = "";
        public string HouseName { get; set; } = "";
        public string HouseAddress { get; set; } = "";
        public string TenantName { get; set; } = "";
        public string TenantPhone { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string? ImagePath { get; set; }
        public string Status { get; set; } = "";
        public string AssignedManagerName { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
    }
}
