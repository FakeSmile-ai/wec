using Microsoft.EntityFrameworkCore;
using MatchesService.Data;
using MatchesService.Models;

namespace MatchesService.Repositories
{
    /// <summary>
    /// Implementación de acceso a datos con EF Core.
    /// </summary>
    public class MatchRepository : IMatchRepository
    {
        private readonly MatchesDbContext _context;
        public MatchRepository(MatchesDbContext context) => _context = context;

        public async Task<List<Match>> GetAllAsync()
        {
            return await _context.Matches
                .AsNoTracking()
                .OrderByDescending(m => m.DateTime)
                .ToListAsync();
        }

        public async Task<Match?> GetByIdAsync(int id)
        {
            return await _context.Matches.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task AddAsync(Match match) => await _context.Matches.AddAsync(match);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
