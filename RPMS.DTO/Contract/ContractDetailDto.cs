using System;

namespace RPMS.DTO.Contract
{
    public class ContractDetailDto : ContractDto
    {
        public decimal Deposit { get; set; }
        public decimal ElectricPrice { get; set; }
        public decimal WaterPrice { get; set; }
        public DateTime? MoveInDate { get; set; }
        public DateTime? MoveOutDate { get; set; }
        public string CreatedByName { get; set; } = "";

        public decimal? PendingMonthlyRent { get; set; }
        public decimal? PendingElectricPrice { get; set; }
        public decimal? PendingWaterPrice { get; set; }
        public decimal? PendingDeposit { get; set; }
        public DateTime? PendingEndDate { get; set; }
        public string? PendingEditNote { get; set; }
        public DateTime? PendingEditAt { get; set; }
        public DateTime? PriceEffectiveDate { get; set; }
    }
}