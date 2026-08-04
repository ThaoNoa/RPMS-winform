using System;
using System.Collections.Generic;

namespace RPMS.DAL.Entities
{
    public class Contract
    {
        public Contract()
        {
            MeterReadings = new HashSet<MeterReading>();
            Invoices = new HashSet<Invoice>();
            MaintenanceRequests = new HashSet<MaintenanceRequest>();
        }

        public int ContractID { get; set; }
        public string ContractCode { get; set; } = "";
        public int RoomID { get; set; }
        public int? TenantID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime MoveInDate { get; set; }
        public DateTime? MoveOutDate { get; set; }
        public decimal Deposit { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal ElectricPrice { get; set; }
        public decimal WaterPrice { get; set; }
        public string Status { get; set; } = "";
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        // Sửa HĐ chờ khách xác nhận
        public decimal? PendingMonthlyRent { get; set; }
        public decimal? PendingElectricPrice { get; set; }
        public decimal? PendingWaterPrice { get; set; }
        public decimal? PendingDeposit { get; set; }
        public DateTime? PendingEndDate { get; set; }
        public string? PendingEditStatus { get; set; }
        public string? PendingEditNote { get; set; }
        public DateTime? PendingEditAt { get; set; }

        // Giá trước khi xác nhận (để tính ngày cũ)
        public decimal? PreviousMonthlyRent { get; set; }
        public decimal? PreviousElectricPrice { get; set; }
        public decimal? PreviousWaterPrice { get; set; }
        public DateTime? PriceEffectiveDate { get; set; }

        public virtual Room Room { get; set; } = null!;
        public virtual User? Tenant { get; set; }
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual Review? Review { get; set; }
        public virtual ICollection<MeterReading> MeterReadings { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
        public virtual ICollection<MaintenanceRequest> MaintenanceRequests { get; set; }
    }
}