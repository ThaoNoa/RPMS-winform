namespace RPMS.DTO.Auth
{
    public class LoginResponseDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = "";
        public string Username { get; set; } = "";
        public int RoleID { get; set; }
        public string RoleName { get; set; } = "";
        public string Token { get; set; } = "";
    }
}