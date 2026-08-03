using RPMS.DTO.Post;
using RPMS.DTO.Tenant;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface ITenantService
    {
        Task<TenantDashboardDto> GetTenantDashboardAsync(int tenantId);
        Task<IEnumerable<PostDto>> SearchRoomsAsync(RoomSearchFilterDto filter);
        Task<bool> SendContractRequestAsync(int tenantId, int contractId, string requestType, string details);
    }
}
