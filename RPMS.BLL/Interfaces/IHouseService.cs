using RPMS.DTO.House;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface IHouseService
    {
        Task<IEnumerable<HouseDto>> GetAllHousesAsync();
        Task<IEnumerable<HouseDto>> GetHousesByOwnerAsync(int ownerId);
        Task<HouseDto> GetHouseByIdAsync(int id);
        Task<HouseDto> CreateHouseAsync(CreateHouseDto request);
        Task<HouseDto> UpdateHouseAsync(int id, UpdateHouseDto request);
        Task<bool> DeleteHouseAsync(int id);
    }
}