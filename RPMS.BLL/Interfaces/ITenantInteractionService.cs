using RPMS.DTO.Tenant;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface ITenantInteractionService
    {
        Task<AppointmentDto> BookAppointmentAsync(CreateAppointmentDto request);
        Task<bool> ToggleFavoriteAsync(int userId, int roomId);
        Task<IEnumerable<FavoriteDto>> GetFavoritesAsync(int userId);
        Task<bool> RemoveFavoriteAsync(int userId, int roomId);
    }
}
