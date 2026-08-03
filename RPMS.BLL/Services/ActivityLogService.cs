using RPMS.BLL.Interfaces;
using RPMS.DAL.Entities;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.ActivityLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ActivityLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LogAsync(int userId, string action, string? details = null, string? ipAddress = null)
        {
            await _unitOfWork.ActivityLogs.AddAsync(new ActivityLog
            {
                UserID = userId,
                Action = action,
                Details = details,
                IPAddress = ipAddress,
                CreatedDate = DateTime.Now
            });
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ActivityLogDto>> GetRecentAsync(int take = 100)
        {
            var items = await _unitOfWork.ActivityLogs.GetAllAsync("User");
            return items.OrderByDescending(x => x.CreatedDate).Take(take).Select(Map).ToList();
        }

        public async Task<IEnumerable<ActivityLogDto>> GetByUserAsync(int userId, int take = 50)
        {
            var items = await _unitOfWork.ActivityLogs.FindAsync(x => x.UserID == userId, "User");
            return items.OrderByDescending(x => x.CreatedDate).Take(take).Select(Map).ToList();
        }

        private static ActivityLogDto Map(ActivityLog x) => new()
        {
            LogID = x.LogID,
            UserID = x.UserID,
            UserName = x.User?.FullName ?? "",
            Action = x.Action,
            Details = x.Details,
            IPAddress = x.IPAddress,
            CreatedDate = x.CreatedDate
        };
    }
}
