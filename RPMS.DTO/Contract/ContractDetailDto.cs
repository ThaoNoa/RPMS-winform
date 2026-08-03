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
    }
}