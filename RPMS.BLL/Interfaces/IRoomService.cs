using RPMS.DTO.Room;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomDto>> GetRoomsByHouseAsync(int houseId);
        Task<RoomDetailDto> GetRoomDetailAsync(int roomId);
        Task<RoomDto> CreateRoomAsync(CreateRoomDto request);
        Task<RoomDto> UpdateRoomAsync(int id, UpdateRoomDto request);
        Task<bool> DeleteRoomAsync(int id);
        Task<bool> UpdateRoomStatusAsync(int id, string status);
        Task<bool> UploadRoomImagesAsync(int roomId, List<string> imagePaths);
        Task<bool> AssignAmenitiesAsync(int roomId, List<int> amenityIds);
    }
}