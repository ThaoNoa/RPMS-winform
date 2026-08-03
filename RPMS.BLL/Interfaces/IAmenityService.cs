using RPMS.DTO.Amenity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface IAmenityService
    {
        Task<IEnumerable<AmenityDto>> GetAllAmenitiesAsync();
        Task<AmenityDto> CreateAmenityAsync(CreateAmenityDto request);
        Task<bool> DeleteAmenityAsync(int id);
    }
}