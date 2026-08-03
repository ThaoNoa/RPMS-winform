using System;

namespace RPMS.DTO.Contract
{
    public class CreateContractDto
    {
        public int RoomID { get; set; }
        public int TenantID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Deposit { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal ElectricPrice { get; set; }
        public decimal WaterPrice { get; set; }
    }
}