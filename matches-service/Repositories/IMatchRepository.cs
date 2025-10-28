using MatchesService.Models;

namespace MatchesService.Repositories
{
    /// <summary>
    /// Acceso a datos para Matches y sus entidades relacionadas.
    /// </summary>
    public interface IMatchRepository
    {
        Task<List<Match>> GetAllAsync();
        Task<Match?> GetByIdAsync(int id);
        Task AddAsync(Match match);
        Task SaveChangesAsync();
    }
}
