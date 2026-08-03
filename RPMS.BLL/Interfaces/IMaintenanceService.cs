using RPMS.DTO.Maintenance;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface IMaintenanceService
    {
        Task<IEnumerable<MaintenanceRequestDto>> GetRequestsByHouseAsync(int houseId);
        Task<IEnumerable<MaintenanceRequestDto>> GetRequestsByTenantAsync(int tenantId);
        Task<MaintenanceRequestDto> CreateRequestAsync(CreateMaintenanceDto request);
        Task<bool> UpdateRequestStatusAsync(int requestId, string status, int managerId);

        Task<IEnumerable<MaintenanceRequestDto>> GetRequestsForManagerAsync(int managerId);
        Task<bool> DeleteRequestAsync(int requestId);
        Task<bool> SendMaintenanceNotificationAsync(int requestId, string message);
    }
}