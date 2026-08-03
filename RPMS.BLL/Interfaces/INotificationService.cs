using RPMS.DTO.Notification;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetByUserAsync(int userId, bool? isRead = null, string? keyword = null);
        Task<int> GetUnreadCountAsync(int userId);
        Task<NotificationDto?> GetByIdAsync(int notificationId);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<bool> DeleteAsync(int notificationId);
        Task<bool> CreateAsync(CreateNotificationDto request);
    }
}
