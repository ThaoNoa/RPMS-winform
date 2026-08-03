using System;

namespace RPMS.DTO.Maintenance
{
    public class MaintenanceRequestDto
    {
        public int RequestID { get; set; }
        public string ContractCode { get; set; } = "";
        public string RoomNumber { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "";
        public string AssignedManagerName { get; set; } = "";
        public DateTime CreatedDate { get; set; }
    }
}