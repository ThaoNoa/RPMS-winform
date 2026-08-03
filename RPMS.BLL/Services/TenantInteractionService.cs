using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.DAL.Entities;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class TenantInteractionService : ITenantInteractionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TenantInteractionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AppointmentDto> BookAppointmentAsync(CreateAppointmentDto request)
        {
            if (request.AppointmentDate <= DateTime.Now)
                throw new BadRequestException("Thời gian hẹn phải trong tương lai.");

            var appointment = new Appointment
            {
                RoomID = request.RoomID,
                TenantID = request.TenantID,
                AppointmentDate = request.AppointmentDate,
                Note = request.Note,
                Status = "Pending",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            await _unitOfWork.Appointments.AddAsync(appointment);

            var room = await _unitOfWork.Rooms.FirstOrDefaultAsync(r => r.RoomID == request.RoomID, "House");
            if (room != null)
            {
                var notif = new Notification
                {
                    UserID = room.House.OwnerID,
                    Title = "Có lịch hẹn xem phòng mới",
                    Content = $"Khách hàng muốn xem phòng {room.RoomNumber} vào ngày {request.AppointmentDate:dd/MM/yyyy HH:mm}",
                    IsRead = false,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                await _unitOfWork.Notifications.AddAsync(notif);
            }

            await _unitOfWork.SaveChangesAsync();

            return new AppointmentDto
            {
                AppointmentID = appointment.AppointmentID,
                RoomID = appointment.RoomID,
                TenantID = appointment.TenantID,
                AppointmentDate = appointment.AppointmentDate,
                Note = appointment.Note,
                Status = appointment.Status
            };
        }

        public async Task<bool> ToggleFavoriteAsync(int userId, int roomId)
        {
            var favorite = await _unitOfWork.Favorites.FirstOrDefaultAsync(f => f.UserID == userId && f.RoomID == roomId);
            if (favorite != null)
            {
                _unitOfWork.Favorites.Remove(favorite);
                await _unitOfWork.SaveChangesAsync();
                return false;
            }

            await _unitOfWork.Favorites.AddAsync(new Favorite
            {
                UserID = userId,
                RoomID = roomId,
                CreatedDate = DateTime.Now
            });
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<FavoriteDto>> GetFavoritesAsync(int userId)
        {
            var favorites = await _unitOfWork.Favorites.FindAsync(
                f => f.UserID == userId,
                "Room.House");

            return favorites
                .OrderByDescending(f => f.CreatedDate)
                .Select(f => new FavoriteDto
                {
                    FavoriteID = f.FavoriteID,
                    RoomID = f.RoomID,
                    RoomNumber = f.Room?.RoomNumber ?? "",
                    HouseName = f.Room?.House?.HouseName ?? "",
                    HouseAddress = f.Room?.House?.Address ?? "",
                    Price = f.Room?.Price ?? 0,
                    Area = f.Room?.Area ?? 0,
                    Status = f.Room?.Status ?? "",
                    CreatedDate = f.CreatedDate
                })
                .ToList();
        }

        public async Task<bool> RemoveFavoriteAsync(int userId, int roomId)
        {
            var favorite = await _unitOfWork.Favorites.FirstOrDefaultAsync(f => f.UserID == userId && f.RoomID == roomId);
            if (favorite == null) return false;
            _unitOfWork.Favorites.Remove(favorite);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
