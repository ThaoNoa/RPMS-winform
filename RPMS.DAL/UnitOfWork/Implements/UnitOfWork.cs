using Microsoft.EntityFrameworkCore.Storage;
using RPMS.DAL.Data;
using RPMS.DAL.Repositories.Implements;
using RPMS.DAL.Repositories.Interfaces;
using RPMS.DAL.UnitOfWork.Interfaces;
using System;
using System.Threading.Tasks;

namespace RPMS.DAL.UnitOfWork.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly RPMSContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        private IRoleRepository? _roles;
        private IUserRepository? _users;
        private IHouseRepository? _houses;
        private IRoomRepository? _rooms;
        private IRoomImageRepository? _roomImages;
        private IAmenityRepository? _amenities;
        private IRoomAmenityRepository? _roomAmenities;
        private IPostRepository? _posts;
        private IPostImageRepository? _postImages;
        private IFavoriteRepository? _favorites;
        private IAppointmentRepository? _appointments;
        private IContractRepository? _contracts;
        private IReviewRepository? _reviews;
        private IMeterReadingRepository? _meterReadings;
        private IInvoiceRepository? _invoices;
        private IPaymentRepository? _payments;
        private IMaintenanceRequestRepository? _maintenanceRequests;
        private IAssignmentRepository? _assignments;
        private INotificationRepository? _notifications;
        private IActivityLogRepository? _activityLogs;
        private IChatConversationRepository? _chatConversations;
        private IChatMessageRepository? _chatMessages;

        public UnitOfWork(RPMSContext context)
        {
            _context = context;
        }

        public IRoleRepository Roles => _roles ??= new RoleRepository(_context);
        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IHouseRepository Houses => _houses ??= new HouseRepository(_context);
        public IRoomRepository Rooms => _rooms ??= new RoomRepository(_context);
        public IRoomImageRepository RoomImages => _roomImages ??= new RoomImageRepository(_context);
        public IAmenityRepository Amenities => _amenities ??= new AmenityRepository(_context);
        public IRoomAmenityRepository RoomAmenities => _roomAmenities ??= new RoomAmenityRepository(_context);
        public IPostRepository Posts => _posts ??= new PostRepository(_context);
        public IPostImageRepository PostImages => _postImages ??= new PostImageRepository(_context);
        public IFavoriteRepository Favorites => _favorites ??= new FavoriteRepository(_context);
        public IAppointmentRepository Appointments => _appointments ??= new AppointmentRepository(_context);
        public IContractRepository Contracts => _contracts ??= new ContractRepository(_context);
        public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);
        public IMeterReadingRepository MeterReadings => _meterReadings ??= new MeterReadingRepository(_context);
        public IInvoiceRepository Invoices => _invoices ??= new InvoiceRepository(_context);
        public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);
        public IMaintenanceRequestRepository MaintenanceRequests => _maintenanceRequests ??= new MaintenanceRequestRepository(_context);
        public IAssignmentRepository Assignments => _assignments ??= new AssignmentRepository(_context);
        public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_context);
        public IActivityLogRepository ActivityLogs => _activityLogs ??= new ActivityLogRepository(_context);
        public IChatConversationRepository ChatConversations => _chatConversations ??= new ChatConversationRepository(_context);
        public IChatMessageRepository ChatMessages => _chatMessages ??= new ChatMessageRepository(_context);

        // Các phương thức khác giữ nguyên
        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
            return _transaction;
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                    _transaction?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await _context.DisposeAsync();
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                }
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}