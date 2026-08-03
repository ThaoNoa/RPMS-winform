using System;

namespace RPMS.DTO.Invoice
{
    public class GenerateInvoiceDto
    {
        public int ContractID { get; set; }
        public DateTime ReadingMonth { get; set; }
        public decimal NewElectric { get; set; }
        public decimal NewWater { get; set; }
        public decimal OtherFee { get; set; }
        public int CreatedBy { get; set; }
    }
}