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
        public string TenantName { get; set; } = "";
    }
}