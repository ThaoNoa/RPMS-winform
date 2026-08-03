using System;
using System.Collections.Generic;

namespace RPMS.DAL.Entities
{
    public class MeterReading
    {
        public MeterReading()
        {
            Invoices = new HashSet<Invoice>();
        }

        public int ReadingID { get; set; }
        public int ContractID { get; set; }
        public DateTime ReadingMonth { get; set; }
        public decimal OldElectric { get; set; }
        public decimal NewElectric { get; set; }
        public decimal OldWater { get; set; }
        public decimal NewWater { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public virtual Contract Contract { get; set; } = null!;
        public virtual User? CreatedByUser { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
    }
}