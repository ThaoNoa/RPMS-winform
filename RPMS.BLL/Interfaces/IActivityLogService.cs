using RPMS.DTO.ActivityLog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface IActivityLogService
    {
        Task LogAsync(int userId, string action, string? details = null, string? ipAddress = null);
        Task<IEnumerable<ActivityLogDto>> GetRecentAsync(int take = 100);
        Task<IEnumerable<ActivityLogDto>> GetByUserAsync(int userId, int take = 50);
    }
}
