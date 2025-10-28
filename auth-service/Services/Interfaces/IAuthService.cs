using AuthService.Models.Dtos;
namespace AuthService.Services.Interfaces
{


    public interface IAuthService
    {
        Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request);
        Task<string?> RegisterAsync(RegisterRequestDto request);

        // NUEVO: login via GitHub -> emite tu JWT y da rol Admin
        Task<LoginResponseDto> AuthenticateWithGitHubAsync(string githubLogin, string? email);
    }
}
