using AutoMapper;
using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.DAL.Entities;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Amenity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class AmenityService : IAmenityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AmenityService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AmenityDto>> GetAllAmenitiesAsync()
        {
            var amenities = await _unitOfWork.Amenities.GetAllAsync();
            return _mapper.Map<IEnumerable<AmenityDto>>(amenities);
        }

        public async Task<AmenityDto> CreateAmenityAsync(CreateAmenityDto request)
        {
            if (await _unitOfWork.Amenities.ExistsAsync(a => a.AmenityName == request.AmenityName))
                throw new BadRequestException("Tiện ích đã tồn tại.");
            var amenity = _mapper.Map<Amenity>(request);
            await _unitOfWork.Amenities.AddAsync(amenity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<AmenityDto>(amenity);
        }

        public async Task<bool> DeleteAmenityAsync(int id)
        {
            var amenity = await _unitOfWork.Amenities.GetByIdAsync(id);
            if (amenity == null) throw new NotFoundException("Tiện ích", id);
            _unitOfWork.Amenities.Remove(amenity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}