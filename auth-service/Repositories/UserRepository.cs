using Microsoft.EntityFrameworkCore;  
using AuthService.Models.Entities;   
using AuthService.Repositories.Interfaces; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Data;

namespace AuthService.Repositories
{
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<User?> GetUserWithRoleAsync(string username, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
                return null;

            return user;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public Task<User?> GetByIdWithRoleAsync(int id) =>
        _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

    }
}