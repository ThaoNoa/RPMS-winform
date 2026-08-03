using Microsoft.EntityFrameworkCore.Storage;
using RPMS.DAL.Repositories.Interfaces;
using System.Threading.Tasks;

namespace RPMS.DAL.UnitOfWork.Interfaces
{
    public interface IUnitOfWork
    {
        IRoleRepository Roles { get; }
        IUserRepository Users { get; }
        IHouseRepository Houses { get; }
        IRoomRepository Rooms { get; }
        IRoomImageRepository RoomImages { get; }
        IAmenityRepository Amenities { get; }
        IRoomAmenityRepository RoomAmenities { get; }
        IPostRepository Posts { get; }
        IPostImageRepository PostImages { get; }
        IFavoriteRepository Favorites { get; }
        IAppointmentRepository Appointments { get; }
        IContractRepository Contracts { get; }
        IReviewRepository Reviews { get; }
        IMeterReadingRepository MeterReadings { get; }
        IInvoiceRepository Invoices { get; }
        IPaymentRepository Payments { get; }
        IMaintenanceRequestRepository MaintenanceRequests { get; }
        IAssignmentRepository Assignments { get; }
        INotificationRepository Notifications { get; }
        IActivityLogRepository ActivityLogs { get; }
        IChatConversationRepository ChatConversations { get; }
        IChatMessageRepository ChatMessages { get; }

        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        void Dispose();
        ValueTask DisposeAsync();
    }
}