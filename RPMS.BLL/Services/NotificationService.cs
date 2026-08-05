using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.Common.Constants;
using RPMS.DAL.Entities;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<NotificationDto>> GetByUserAsync(int userId, bool? isRead = null, string? keyword = null)
        {
            var items = await _unitOfWork.Notifications.FindAsync(n => n.UserID == userId, "User");
            var query = items.AsEnumerable();

            if (isRead.HasValue)
                query = query.Where(n => n.IsRead == isRead.Value);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var key = keyword.Trim().ToLowerInvariant();
                query = query.Where(n =>
                    (n.Title ?? "").ToLowerInvariant().Contains(key) ||
                    (n.Content ?? "").ToLowerInvariant().Contains(key));
            }

            return query
                .OrderByDescending(n => n.CreatedDate)
                .Select(Map)
                .ToList();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _unitOfWork.Notifications.CountAsync(n => n.UserID == userId && !n.IsRead);
        }

        public async Task<NotificationDto?> GetByIdAsync(int notificationId)
        {
            var item = await _unitOfWork.Notifications.FirstOrDefaultAsync(n => n.NotificationID == notificationId, "User");
            return item == null ? null : Map(item);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var item = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (item == null) throw new NotFoundException("Thông báo", notificationId);
            item.IsRead = true;
            item.UpdatedDate = DateTime.Now;
            _unitOfWork.Notifications.Update(item);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var items = await _unitOfWork.Notifications.FindAsync(n => n.UserID == userId && !n.IsRead);
            foreach (var item in items)
            {
                item.IsRead = true;
                item.UpdatedDate = DateTime.Now;
                _unitOfWork.Notifications.Update(item);
            }
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int notificationId)
        {
            var item = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (item == null) return false;
            _unitOfWork.Notifications.Remove(item);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateAsync(CreateNotificationDto request)
        {
            var entity = BuildEntity(request);
            await _unitOfWork.Notifications.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteRelatedActionsAsync(string actionType, int relatedId, string newStatus)
        {
            var items = await _unitOfWork.Notifications.FindAsync(n =>
                n.RelatedID == relatedId
                && n.ActionType == actionType
                && n.ActionStatus == NotificationActions.Pending);
            foreach (var item in items)
            {
                item.ActionStatus = newStatus;
                item.IsRead = true;
                item.UpdatedDate = DateTime.Now;
                _unitOfWork.Notifications.Update(item);
            }
            if (items.Any())
                await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public static Notification BuildEntity(CreateNotificationDto request) => new()
        {
            UserID = request.UserID,
            Title = request.Title,
            Content = request.Content,
            ActionType = request.ActionType,
            RelatedID = request.RelatedID,
            ActionStatus = request.ActionStatus,
            IsRead = false,
            CreatedDate = DateTime.Now,
            UpdatedDate = DateTime.Now
        };

        private static NotificationDto Map(Notification n) => new()
        {
            NotificationID = n.NotificationID,
            UserID = n.UserID,
            Title = n.Title,
            Content = n.Content,
            IsRead = n.IsRead,
            ActionType = n.ActionType,
            RelatedID = n.RelatedID,
            ActionStatus = n.ActionStatus,
            CreatedDate = n.CreatedDate,
            UpdatedDate = n.UpdatedDate,
            UserName = n.User?.FullName ?? ""
        };
    }
}
