using RPMS.DTO.Report;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface IReportService
    {
        Task<ReportSummaryDto> GetAdminReportAsync();
        Task<ReportSummaryDto> GetLandlordReportAsync(int landlordId);
    }
}
