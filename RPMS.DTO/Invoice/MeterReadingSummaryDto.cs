using System;

namespace RPMS.DTO.Invoice
{
    public class MeterReadingSummaryDto
    {
        public DateTime ReadingMonth { get; set; }
        public decimal OldElectric { get; set; }
        public decimal NewElectric { get; set; }
        public decimal OldWater { get; set; }
        public decimal NewWater { get; set; }
    }
}
