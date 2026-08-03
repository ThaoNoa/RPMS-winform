namespace RPMS.DTO.Invoice
{
    public class ProcessPaymentDto
    {
        public decimal Amount { get; set; }
        public string Method { get; set; } = "";
    }
}