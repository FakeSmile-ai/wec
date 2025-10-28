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

        // ================= LISTADOS =================
        public async Task<IEnumerable<Match>> GetAllAsync(int page, int pageSize, string? status, int? teamId, DateTime? from, DateTime? to)
        {
            var query = _context.Matches.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(m => m.Status == status);

            if (teamId.HasValue)
                query = query.Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId);

            if (from.HasValue)
                query = query.Where(m => m.DateMatch >= from.Value);

            if (to.HasValue)
                query = query.Where(m => m.DateMatch <= to.Value);

            return await query
                .OrderByDescending(m => m.DateMatch)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountAsync(string? status, int? teamId, DateTime? from, DateTime? to)
        {
            var query = _context.Matches.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(m => m.Status == status);

            if (teamId.HasValue)
                query = query.Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId);

            if (from.HasValue)
                query = query.Where(m => m.DateMatch >= from.Value);

            if (to.HasValue)
                query = query.Where(m => m.DateMatch <= to.Value);

            return await query.CountAsync();
        }

        public async Task<Match?> GetByIdAsync(int id)
        {
            // tracked para modificaciones subsecuentes
            return await _context.Matches
                .Include(m => m.ScoreEvents)
                .Include(m => m.Fouls)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Match>> GetUpcomingAsync()
        {
            var nowUtc = DateTime.UtcNow;
            return await _context.Matches.AsNoTracking()
                .Where(m => m.Status == "Scheduled" && m.DateMatch > nowUtc)
                .OrderBy(m => m.DateMatch)
                .Take(10)
                .ToListAsync();
        }

        public async Task<IEnumerable<Match>> GetByRangeAsync(DateTime from, DateTime to)
        {
            return await _context.Matches.AsNoTracking()
                .Where(m => m.DateMatch >= from && m.DateMatch <= to)
                .OrderBy(m => m.DateMatch)
                .ToListAsync();
        }

        // ================= CRUD MATCH =================
        public async Task AddAsync(Match match) => await _context.Matches.AddAsync(match);
        public Task UpdateAsync(Match match) { _context.Matches.Update(match); return Task.CompletedTask; }
        public Task DeleteAsync(Match match) { _context.Matches.Remove(match); return Task.CompletedTask; }
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        // ================= SCORE EVENTS =================
        public async Task AddScoreEventAsync(ScoreEvent ev) => await _context.ScoreEvents.AddAsync(ev);

        // ================= FOULS =================
        public async Task AddFoulAsync(Foul foul) => await _context.Fouls.AddAsync(foul);

        public async Task<int> GetFoulCountAsync(int matchId, int teamId)
        {
            return await _context.Fouls.AsNoTracking()
                .CountAsync(f => f.MatchId == matchId && f.TeamId == teamId);
        }

        public async Task<int> RemoveLastFoulsAsync(int matchId, int teamId, int count)
        {
            if (count <= 0) return 0;

            var toDelete = await _context.Fouls
                .Where(f => f.MatchId == matchId && f.TeamId == teamId)
                .OrderByDescending(f => f.DateRegister)
                .Take(count)
                .ToListAsync();

            if (toDelete.Count == 0) return 0;

            _context.Fouls.RemoveRange(toDelete);
            return toDelete.Count;
        }
    }
}
