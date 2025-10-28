using AuthService.Models.Entities;

namespace AuthService.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role> AddRoleAsync(Role role);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(int id);
        Task<Role?> UpdateRoleAsync(Role role);
        Task<bool> DeleteRoleAsync(int id);

        Task<Role?> GetByNameAsync(string name);

    }
}