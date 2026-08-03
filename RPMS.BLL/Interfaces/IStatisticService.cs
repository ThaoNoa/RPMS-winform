using RPMS.DTO.Statistic;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface IStatisticService
    {
        Task<AdminDashboardDto> GetAdminDashboardStatsAsync();
        Task<LandlordDashboardDto> GetLandlordDashboardStatsAsync(int landlordId);

        Task<ManagerDashboardDto> GetManagerDashboardStatsAsync(int managerId);
    }
}