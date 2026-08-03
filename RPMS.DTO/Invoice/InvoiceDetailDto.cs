using System;

namespace RPMS.DTO.Invoice
{
    public class InvoiceDetailDto : InvoiceDto
    {
        public decimal Rent { get; set; }
        public decimal ElectricCost { get; set; }
        public decimal WaterCost { get; set; }
        public decimal OtherFee { get; set; }
        public decimal OldElectric { get; set; }
        public decimal NewElectric { get; set; }
        public decimal OldWater { get; set; }
        public decimal NewWater { get; set; }
        public DateTime? ReadingMonth { get; set; }
        public DateTime? PaidDate { get; set; }

        // Khách thuê
        public string TenantName { get; set; } = "";
        public string TenantPhone { get; set; } = "";
        public string TenantEmail { get; set; } = "";

        // Phòng / nhà
        public string HouseName { get; set; } = "";
        public string HouseAddress { get; set; } = "";
        public decimal? RoomArea { get; set; }
        public decimal? RoomPrice { get; set; }

        // Hợp đồng
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public DateTime? MoveInDate { get; set; }
        public DateTime? MoveOutDate { get; set; }
        public decimal ElectricPrice { get; set; }
        public decimal WaterPrice { get; set; }
        public string ContractStatus { get; set; } = "";

        // Prorate tiền nhà
        public decimal FullMonthlyRent { get; set; }
        public int DaysInMonth { get; set; }
        public int OccupiedDays { get; set; }
        public DateTime? OccupancyFrom { get; set; }
        public DateTime? OccupancyTo { get; set; }
        public bool IsProrated { get; set; }
        public string RentNote { get; set; } = "";
    }
}
