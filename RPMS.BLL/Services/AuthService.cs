using AutoMapper;
using RPMS.BLL.Exceptions;
using RPMS.BLL.Helpers;
using RPMS.BLL.Interfaces;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Auth;
using System;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Username == request.Username, "Role");
            if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.Password))
                throw new UnauthorizedException("Tên đăng nhập hoặc mật khẩu không chính xác.");
            if (user.Status != "Active")
                throw new UnauthorizedException("Tài khoản của bạn đã bị khóa.");

            await _unitOfWork.ActivityLogs.AddAsync(new RPMS.DAL.Entities.ActivityLog
            {
                UserID = user.UserID,
                Action = "Login",
                Details = $"Đăng nhập thành công: {user.Username}",
                CreatedDate = DateTime.Now
            });
            await _unitOfWork.SaveChangesAsync();

            return new LoginResponseDto
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Username = user.Username,
                RoleID = user.RoleID,
                RoleName = user.Role.RoleName,
                Token = "JWT_TOKEN_MOCK"
            };
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto request)
        {
            if (request.NewPassword != request.ConfirmNewPassword)
                throw new BadRequestException("Mật khẩu xác nhận không khớp.");
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("Người dùng", userId);
            if (!PasswordHelper.VerifyPassword(request.OldPassword, user.Password))
                throw new BadRequestException("Mật khẩu cũ không chính xác.");
            user.Password = PasswordHelper.HashPassword(request.NewPassword);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) throw new NotFoundException("Email", email);
            user.Password = PasswordHelper.HashPassword(newPassword);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}