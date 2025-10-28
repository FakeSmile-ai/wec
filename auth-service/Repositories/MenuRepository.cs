using Microsoft.EntityFrameworkCore;  
using AuthService.Models.Entities;   
using AuthService.Repositories.Interfaces; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Data;

namespace AuthService.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly AppDbContext _db;

        // Constructor
        public MenuRepository(AppDbContext db)
        {
            _db = db;
        }

        // Método para obtener todos los menús
        public async Task<List<Menu>> GetAllAsync()
        {
            return await _db.Menus.OrderBy(m => m.Name).ToListAsync();  // Usamos ToListAsync para obtener todos los menús de manera asincrónica
        }

        // Método para obtener los menús por RoleId
        public async Task<List<Menu>> GetByRoleIdAsync(int roleId)
        {
            return await _db.RoleMenus
                .Where(rm => rm.RoleId == roleId)
                .Select(rm => rm.Menu)
                .OrderBy(m => m.Name)
                .ToListAsync();  // Usamos ToListAsync aquí también
        }

        // Método para asignar menús a un rol
        public async Task AssignToRoleAsync(int roleId, IEnumerable<int> menuIds)
        {
            var current = await _db.RoleMenus.Where(rm => rm.RoleId == roleId).ToListAsync();
            _db.RoleMenus.RemoveRange(current);

            var toAdd = menuIds.Distinct().Select(mid => new RoleMenu { RoleId = roleId, MenuId = mid });
            await _db.RoleMenus.AddRangeAsync(toAdd);
            await _db.SaveChangesAsync();
        }
    }
}
