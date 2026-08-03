using AutoMapper;
using RPMS.BLL.Exceptions;
using RPMS.BLL.Helpers;
using RPMS.BLL.Interfaces;
using RPMS.DAL.Entities;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync("Role");
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(int roleId)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.RoleID == roleId, "Role");
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.UserID == id, "Role");
            if (user == null) throw new NotFoundException("Người dùng", id);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto request)
        {
            if (await _unitOfWork.Users.ExistsAsync(u => u.Username == request.Username))
                throw new BadRequestException("Tên đăng nhập đã tồn tại.");
            if (!string.IsNullOrEmpty(request.Email) && await _unitOfWork.Users.ExistsAsync(u => u.Email == request.Email))
                throw new BadRequestException("Email đã được sử dụng.");
            var user = _mapper.Map<User>(request);
            user.Password = PasswordHelper.HashPassword(request.Password);
            user.Status = "Active";
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return await GetUserByIdAsync(user.UserID);
        }

        public async Task<UserDto> UpdateUserAsync(int id, UpdateUserDto request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) throw new NotFoundException("Người dùng", id);
            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email && await _unitOfWork.Users.ExistsAsync(u => u.Email == request.Email))
                throw new BadRequestException("Email đã được sử dụng bởi tài khoản khác.");
            user.RoleID = request.RoleID;
            user.FullName = request.FullName;
            user.Phone = request.Phone;
            user.Email = request.Email;
            user.Address = request.Address;
            user.Status = request.Status;
            user.UpdatedDate = DateTime.Now;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return await GetUserByIdAsync(id);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) throw new NotFoundException("Người dùng", id);
            user.Status = "Inactive";
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleUserStatusAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) throw new NotFoundException("Người dùng", id);
            user.Status = user.Status == "Active" ? "Inactive" : "Active";
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}