using System;

namespace RPMS.DAL.Entities
{
    public class MaintenanceRequest
    {
        public int RequestID { get; set; }
        public int ContractID { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string Status { get; set; } = "";
        public int? AssignedManager { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public virtual Contract Contract { get; set; } = null!;
        public virtual User? Manager { get; set; }
    }
}