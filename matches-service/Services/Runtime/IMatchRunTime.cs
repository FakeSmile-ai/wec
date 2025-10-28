namespace MatchesService.Services.Runtime;

public record TimerSnapshot(bool IsRunning, int RemainingSeconds, DateTime? EndsAt);

public interface IMatchRunTime
{
    TimerSnapshot GetOrCreate(int matchId, int defaultSeconds);
    TimerSnapshot Get(int matchId);
    void Start(int matchId, int seconds);
    int Pause(int matchId);
    void Resume(int matchId);
    void Reset(int matchId);
    void Stop(int matchId);
}
