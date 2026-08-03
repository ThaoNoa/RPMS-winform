using RPMS.DAL.Data;
using RPMS.DAL.Entities;
using RPMS.DAL.Repositories.Interfaces;

namespace RPMS.DAL.Repositories.Implements
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(RPMSContext context) : base(context)
        {
        }
    }
}