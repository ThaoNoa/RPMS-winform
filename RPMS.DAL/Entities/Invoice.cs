using System;
using System.Collections.Generic;

namespace RPMS.DAL.Entities
{
    public class Invoice
    {
        public Invoice()
        {
            Payments = new HashSet<Payment>();
        }

        public int InvoiceID { get; set; }
        public string InvoiceCode { get; set; } = "";
        public int ContractID { get; set; }
        public int ReadingID { get; set; }
        public decimal Rent { get; set; }
        public decimal ElectricCost { get; set; }
        public decimal WaterCost { get; set; }
        public decimal OtherFee { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = "";
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public virtual Contract Contract { get; set; } = null!;
        public virtual MeterReading MeterReading { get; set; } = null!;
        public virtual ICollection<Payment> Payments { get; set; }
    }
}