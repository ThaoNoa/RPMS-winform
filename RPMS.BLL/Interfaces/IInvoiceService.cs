using RPMS.DTO.Invoice;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface IInvoiceService
    {
        Task<IEnumerable<InvoiceDto>> GetInvoicesByContractAsync(int contractId);
        Task<InvoiceDetailDto> GetInvoiceByIdAsync(int id);
        Task<InvoiceDto> GenerateMonthlyInvoiceAsync(GenerateInvoiceDto request);
        Task<bool> ProcessPaymentAsync(int invoiceId, ProcessPaymentDto request);
        Task<MeterReadingSummaryDto?> GetLatestReadingAsync(int contractId);
    }
}