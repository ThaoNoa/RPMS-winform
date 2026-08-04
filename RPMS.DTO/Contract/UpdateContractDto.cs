using System;

namespace RPMS.DTO.Contract
{
    public class UpdateContractDto
    {
        public int ContractID { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Deposit { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal ElectricPrice { get; set; }
        public decimal WaterPrice { get; set; }
        public string? Note { get; set; }
    }
}
