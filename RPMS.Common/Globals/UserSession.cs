using RPMS.DTO.Auth;

namespace RPMS.Common.Globals
{
    public static class UserSession
    {
        public static LoginResponseDto? CurrentUser { get; private set; }

        public static void Login(LoginResponseDto user)
        {
            CurrentUser = user;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }

        public static bool IsLoggedIn => CurrentUser != null;
    }
}