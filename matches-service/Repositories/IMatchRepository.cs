using MatchesService.Models;

namespace MatchesService.Repositories
{
    /// <summary>
    /// Acceso a datos para Matches y sus entidades relacionadas.
    /// </summary>
    public interface IMatchRepository
    {
        // ==== LISTADOS / CONSULTAS ====
        Task<IEnumerable<Match>> GetAllAsync(int page, int pageSize, string? status, int? teamId, DateTime? from, DateTime? to);
        Task<Match?> GetByIdAsync(int id);
        Task<int> CountAsync(string? status, int? teamId, DateTime? from, DateTime? to);
        Task<IEnumerable<Match>> GetUpcomingAsync();
        Task<IEnumerable<Match>> GetByRangeAsync(DateTime from, DateTime to);

        // ==== CRUD MATCH ====
        Task AddAsync(Match match);
        Task UpdateAsync(Match match);
        Task DeleteAsync(Match match);
        Task SaveChangesAsync();

        // ==== SCORE EVENTS ====
        Task AddScoreEventAsync(ScoreEvent ev);

        // ==== FOULS ====
        Task AddFoulAsync(Foul foul);
        Task<int> GetFoulCountAsync(int matchId, int teamId);
        /// <summary>Elimina las últimas 'count' faltas de un equipo en un partido. Retorna cuántas eliminó.</summary>
        Task<int> RemoveLastFoulsAsync(int matchId, int teamId, int count);
    }
}
