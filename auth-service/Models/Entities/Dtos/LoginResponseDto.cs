using AuthService.Models.Dtos; 
namespace AuthService.Models.Dtos
{
    public class LoginResponseDto
    {
        public string? Token { get; set; }
        public string? Username { get; set; }
        public RoleDto? Role { get; set; }
    }
}
