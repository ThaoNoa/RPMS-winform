using System;

namespace RPMS.DTO.Invoice
{
    public class InvoiceDto
    {
        public int InvoiceID { get; set; }
        public string InvoiceCode { get; set; } = "";
        public string ContractCode { get; set; } = "";
        public string RoomNumber { get; set; } = "";
        public decimal Total { get; set; }
        public string Status { get; set; } = "";
        public DateTime? DueDate { get; set; }
    }
}