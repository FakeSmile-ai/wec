using MatchesService.Models.DTOs;

namespace MatchesService.Services
{
    public interface IMatchService
    {
        Task<object> ListarAsync(int page, int pageSize, string? status, int? teamId, DateTime? from, DateTime? to);
        Task<object?> GetMatchAsync(int id);
        Task<object> ProximosAsync();
        Task<(bool Success, string? Error, object? Data)> RangoAsync(DateTime from, DateTime to);

        Task<(bool Success, string? Error, object? Data)> ProgramarAsync(ProgramarPartidoDto dto);
        Task<(bool Success, string? Error, object? Data)> ReprogramarAsync(int id, ReprogramarDto dto);

        Task<(bool Success, string? Error, object? Data)> NewGameAsync(NewGameDto dto);
        Task<(bool Success, string? Error, object? Data)> NewByTeamsAsync(NewGameByTeamsDto dto);

        Task<(bool Success, string? Error, object? Data)> AddScoreAsync(int id, AddScoreDto dto);
        Task<(bool Success, string? Error, object? Data)> AdjustScoreAsync(int id, AdjustScoreDto dto);
        Task<(bool Success, string? Error, object? Data)> AddFoulAsync(int id, AddFoulDto dto);
        Task<(bool Success, string? Error, object? Data)> AdjustFoulAsync(int id, AdjustFoulDto dto);

        Task<(bool Success, string? Error, object? Data)> StartTimerAsync(int id, StartTimerDto? dto);
        Task<(bool Success, string? Error, object? Data)> PauseTimerAsync(int id);
        Task<(bool Success, string? Error, object? Data)> ResumeTimerAsync(int id);
        Task<(bool Success, string? Error, object? Data)> ResetTimerAsync(int id);

        Task<(bool Success, string? Error, object? Data, object? GameEnded)> AdvanceQuarterAsync(int id);
        Task<(bool Success, string? Error, object? Data, object? GameEnded)> AutoAdvanceQuarterAsync(int id);
        Task<(bool Success, string? Error, object? Data, object? GameEnded)> FinishAsync(int id, FinishMatchDto dto);

        Task<(bool Success, string? Error, object? Data)> CancelAsync(int id);
        Task<(bool Success, string? Error, object? Data)> SuspendAsync(int id);
    }
}
