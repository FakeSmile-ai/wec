using MatchesService.Models.DTOs;

namespace MatchesService.Services
{
    public interface IMatchService
    {
        Task<IReadOnlyList<MatchDto>> GetMatchesAsync(CancellationToken cancellationToken = default);
        Task<MatchDto?> GetMatchAsync(int id, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error, MatchDto? Match)> ProgramMatchAsync(MatchProgramRequest request, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error, MatchDto? Match)> UpdateScoreAsync(int matchId, ScoreRequest request, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error, MatchDto? Match)> RegisterFoulAsync(int matchId, FoulRequest request, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error, MatchDto? Match)> UpdateTimerAsync(int matchId, TimerRequest request, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error, MatchDto? Match)> AdvanceQuarterAsync(int matchId, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error, MatchDto? Match)> FinishMatchAsync(int matchId, FinishMatchRequest request, CancellationToken cancellationToken = default);
    }
}
