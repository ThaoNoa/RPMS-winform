using AutoMapper;
using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.DAL.Entities;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RoomService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoomDto>> GetRoomsByHouseAsync(int houseId)
        {
            var rooms = await _unitOfWork.Rooms.FindAsync(r => r.HouseID == houseId, "House");
            return _mapper.Map<IEnumerable<RoomDto>>(rooms);
        }

        public async Task<RoomDetailDto> GetRoomDetailAsync(int roomId)
        {
            var room = await _unitOfWork.Rooms.FirstOrDefaultAsync(
                r => r.RoomID == roomId,
                "House, RoomImages, RoomAmenities.Amenity");
            if (room == null) throw new NotFoundException("Phòng", roomId);
            return _mapper.Map<RoomDetailDto>(room);
        }

        public async Task<RoomDto> CreateRoomAsync(CreateRoomDto request)
        {
            if (await _unitOfWork.Rooms.ExistsAsync(r => r.HouseID == request.HouseID && r.RoomNumber == request.RoomNumber))
                throw new BadRequestException($"Phòng {request.RoomNumber} đã tồn tại trong nhà này.");
            var room = _mapper.Map<Room>(request);
            room.Status = "Available";
            await _unitOfWork.Rooms.AddAsync(room);
            await _unitOfWork.SaveChangesAsync();
            var createdRoom = await _unitOfWork.Rooms.FirstOrDefaultAsync(r => r.RoomID == room.RoomID, "House");
            return _mapper.Map<RoomDto>(createdRoom);
        }

        public async Task<RoomDto> UpdateRoomAsync(int id, UpdateRoomDto request)
        {
            var room = await _unitOfWork.Rooms.FirstOrDefaultAsync(r => r.RoomID == id, "House");
            if (room == null) throw new NotFoundException("Phòng", id);
            if (room.RoomNumber != request.RoomNumber && await _unitOfWork.Rooms.ExistsAsync(r => r.HouseID == room.HouseID && r.RoomNumber == request.RoomNumber))
                throw new BadRequestException($"Phòng {request.RoomNumber} đã tồn tại.");
            room.RoomNumber = request.RoomNumber;
            room.Floor = request.Floor;
            room.Area = request.Area;
            room.Price = request.Price;
            room.Capacity = request.Capacity;
            room.Bedroom = request.Bedroom;
            room.Bathroom = request.Bathroom;
            room.Furniture = request.Furniture;
            room.Description = request.Description;
            room.Status = request.Status;
            room.UpdatedDate = DateTime.Now;
            _unitOfWork.Rooms.Update(room);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<RoomDto>(room);
        }

        public async Task<bool> DeleteRoomAsync(int id)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(id);
            if (room == null) throw new NotFoundException("Phòng", id);
            if (room.Status == "Occupied")
                throw new BadRequestException("Phòng đang có người thuê, không thể xóa.");
            _unitOfWork.Rooms.Remove(room);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateRoomStatusAsync(int id, string status)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(id);
            if (room == null) throw new NotFoundException("Phòng", id);
            room.Status = status;
            room.UpdatedDate = DateTime.Now;
            _unitOfWork.Rooms.Update(room);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UploadRoomImagesAsync(int roomId, List<string> imagePaths)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(roomId);
            if (room == null) throw new NotFoundException("Phòng", roomId);
            var existingImages = await _unitOfWork.RoomImages.FindAsync(ri => ri.RoomID == roomId);
            _unitOfWork.RoomImages.RemoveRange(existingImages);
            var newImages = imagePaths.Select((path, index) => new RoomImage
            {
                RoomID = roomId,
                ImagePath = path,
                DisplayOrder = index + 1
            }).ToList();
            await _unitOfWork.RoomImages.AddRangeAsync(newImages);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignAmenitiesAsync(int roomId, List<int> amenityIds)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var room = await _unitOfWork.Rooms.GetByIdAsync(roomId);
                if (room == null) throw new NotFoundException("Phòng", roomId);
                var existing = await _unitOfWork.RoomAmenities.FindAsync(ra => ra.RoomID == roomId);
                _unitOfWork.RoomAmenities.RemoveRange(existing);
                var newRoomAmenities = amenityIds.Select(aId => new RoomAmenity { RoomID = roomId, AmenityID = aId }).ToList();
                await _unitOfWork.RoomAmenities.AddRangeAsync(newRoomAmenities);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}