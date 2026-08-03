using AutoMapper;
using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.DAL.Entities;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.House;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class HouseService : IHouseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public HouseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<HouseDto>> GetAllHousesAsync()
        {
            var houses = await _unitOfWork.Houses.GetAllAsync("Owner, Rooms");
            return _mapper.Map<IEnumerable<HouseDto>>(houses);
        }

        public async Task<IEnumerable<HouseDto>> GetHousesByOwnerAsync(int ownerId)
        {
            var houses = await _unitOfWork.Houses.FindAsync(h => h.OwnerID == ownerId, "Owner, Rooms");
            return _mapper.Map<IEnumerable<HouseDto>>(houses);
        }

        public async Task<HouseDto> GetHouseByIdAsync(int id)
        {
            var house = await _unitOfWork.Houses.FirstOrDefaultAsync(h => h.HouseID == id, "Owner, Rooms");
            if (house == null) throw new NotFoundException("Nhà", id);
            return _mapper.Map<HouseDto>(house);
        }

        public async Task<HouseDto> CreateHouseAsync(CreateHouseDto request)
        {
            var owner = await _unitOfWork.Users.GetByIdAsync(request.OwnerID);
            if (owner == null) throw new NotFoundException("Chủ nhà", request.OwnerID);
            var house = _mapper.Map<House>(request);
            house.Status = "Active";
            await _unitOfWork.Houses.AddAsync(house);
            await _unitOfWork.SaveChangesAsync();
            return await GetHouseByIdAsync(house.HouseID);
        }

        public async Task<HouseDto> UpdateHouseAsync(int id, UpdateHouseDto request)
        {
            var house = await _unitOfWork.Houses.GetByIdAsync(id);
            if (house == null) throw new NotFoundException("Nhà", id);
            house.HouseName = request.HouseName;
            house.Address = request.Address;
            house.Description = request.Description;
            house.Status = request.Status;
            house.UpdatedDate = DateTime.Now;
            _unitOfWork.Houses.Update(house);
            await _unitOfWork.SaveChangesAsync();
            return await GetHouseByIdAsync(id);
        }

        public async Task<bool> DeleteHouseAsync(int id)
        {
            var house = await _unitOfWork.Houses.FirstOrDefaultAsync(h => h.HouseID == id, "Rooms");
            if (house == null) throw new NotFoundException("Nhà", id);
            if (house.Rooms.Count > 0)
                throw new BadRequestException("Không thể xóa nhà đang có phòng. Vui lòng xóa các phòng trước.");
            _unitOfWork.Houses.Remove(house);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}