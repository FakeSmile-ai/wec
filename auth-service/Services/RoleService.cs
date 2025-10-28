using AuthService.Models.Entities;
using AuthService.Models.Dtos;
using AuthService.Repositories.Interfaces;
using AuthService.Services.Interfaces;

namespace AuthService.Services
{
    public class RoleService(IRoleRepository repository) : IRoleService
    {
        private readonly IRoleRepository _repository = repository;

        public async Task<RoleReadDto> CreateRoleAsync(RoleCreateDto dto)
        {
            var entity = new Role
            {
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy
            };

            var created = await _repository.AddRoleAsync(entity);
            return MapToReadDto(created);
        }

        public async Task<IEnumerable<RoleReadDto>> GetAllRolesAsync()
        {
            var roles = await _repository.GetAllRolesAsync();
            return roles.Select(MapToReadDto);
        }

        public async Task<RoleReadDto?> GetRoleByIdAsync(int id)
        {
            var entity = await _repository.GetRoleByIdAsync(id);
            return entity == null ? null : MapToReadDto(entity);
        }

        public async Task<RoleReadDto?> UpdateRoleAsync(int id, RoleUpdateDto dto)
        {
            var entity = new Role
            {
                Id = id,
                Name = dto.Name,
                UpdatedBy = dto.UpdatedBy
            };

            var updated = await _repository.UpdateRoleAsync(entity);
            return updated == null ? null : MapToReadDto(updated);
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            return await _repository.DeleteRoleAsync(id);
        }

        private static RoleReadDto MapToReadDto(Role role) => new()
        {
            Id = role.Id,
            Name = role.Name,
            CreatedAt = role.CreatedAt,
            CreatedBy = role.CreatedBy,
            UpdatedAt = role.UpdatedAt,
            UpdatedBy = role.UpdatedBy
        };
    }
}