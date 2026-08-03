using RPMS.DTO.Auth;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto request);
        Task<bool> ResetPasswordAsync(string email, string newPassword);
    }
}