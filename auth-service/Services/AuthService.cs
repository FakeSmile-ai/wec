using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using AuthService.Models.Dtos;
using AuthService.Models.Entities;
using AuthService.Repositories.Interfaces;
using AuthService.Services.Interfaces;

namespace AuthService.Services
{
    public class AuthService(IUserRepository userRepository, IRoleRepository roleRepository, IConfiguration config) : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IRoleRepository _roleRepository = roleRepository;
        private readonly IConfiguration _config = config;

        public async Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetUserWithRoleAsync(request.Username, request.Password);
            if (user == null) return null;

            var token = GenerateJwtToken(user);

            return new LoginResponseDto
            {
                Username = user.Username,
                Role = new RoleDto { Id = user.RoleId, Name = user.Role?.Name ?? "" },
                Token = token
            };
        }

        public async Task<string?> RegisterAsync(RegisterRequestDto request)
        {
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null) return null;

            var role = await _roleRepository.GetRoleByIdAsync(request.RoleId);
            if (role == null) return null;

            var hashed = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Password = hashed,
                RoleId = role.Id
            };

            await _userRepository.AddAsync(user);
            return "Usuario registrado correctamente.";
        }

        // === GitHub OAuth -> JWT propio + rol Admin ===
        public async Task<LoginResponseDto> AuthenticateWithGitHubAsync(string githubLogin, string? email)
        {
            var username = $"github:{githubLogin}".ToLowerInvariant();

            var user = await _userRepository.GetByUsernameAsync(username);
            var admin = await EnsureAdminRoleAsync();

            if (user is null)
            {
                var placeholder = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N"));
                user = new User
                {
                    Username = username,
                    Password = placeholder,
                    RoleId = admin.Id
                };
                await _userRepository.AddAsync(user);
                user.Role = admin;
            }
            else
            {
                user.Role ??= admin;
                if (user.RoleId != admin.Id)
                {
                    user.RoleId = admin.Id;
                    // Si tienes método de update, podrías persistir esto aquí.
                }
            }

            var token = GenerateJwtToken(user);

            return new LoginResponseDto
            {
                Username = user.Username,
                Role = new RoleDto { Id = user.RoleId, Name = user.Role?.Name ?? "Admin" },
                Token = token
            };
        }

        private async Task<Role> EnsureAdminRoleAsync()
        {
            var admin = await _roleRepository.GetByNameAsync("Admin");
            if (admin is not null) return admin;

            return await _roleRepository.AddRoleAsync(new Role
            {
                Name = "Admin",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 0
            });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var keyStr = jwtSettings["Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
            var keyBytes = Encoding.UTF8.GetBytes(keyStr);
            if (keyBytes.Length < 32) throw new InvalidOperationException("Jwt:Key debe tener al menos 32 bytes (256 bits).");

            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roleName = string.IsNullOrWhiteSpace(user.Role?.Name) ? "User" : user.Role!.Name;
            if (roleName.Equals("administrador", StringComparison.OrdinalIgnoreCase)) roleName = "Admin";
            if (roleName.Equals("admin", StringComparison.OrdinalIgnoreCase)) roleName = "Admin";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("role", roleName),
                new Claim("roles", System.Text.Json.JsonSerializer.Serialize(new []{ roleName })),
                new Claim("Id", user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiresInMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
