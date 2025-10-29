using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MatchesService.Hubs;
using MatchesService.Models;
using MatchesService.Models.DTOs;
using MatchesService.Repositories;

namespace MatchesService.Services;

public class MatchService : IMatchService
{
    private const int DefaultQuarterSeconds = 600;

    private readonly IMatchRepository _repository;
    private readonly ITeamClientService _teamClient;
    private readonly IHubContext<MatchHub> _hub;
    private readonly ILogger<MatchService> _logger;

    public MatchService(
        IMatchRepository repository,
        ITeamClientService teamClient,
        IHubContext<MatchHub> hub,
        ILogger<MatchService> logger)
    {
        _repository = repository;
        _teamClient = teamClient;
        _hub = hub;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MatchDto>> GetMatchesAsync(CancellationToken cancellationToken = default)
    {
        var matches = await _repository.GetAllAsync();
        var teamIds = matches
            .SelectMany(m => new[] { m.HomeTeamId, m.AwayTeamId })
            .Distinct();
        var teams = await BuildTeamLookupAsync(teamIds, cancellationToken);
        return matches.Select(m => MapToDto(m, teams)).ToList();
    }

    public async Task<MatchDto?> GetMatchAsync(int id, CancellationToken cancellationToken = default)
    {
        var match = await _repository.GetByIdAsync(id);
        if (match is null) return null;

        var teams = await BuildTeamLookupAsync(
            match is null ? Enumerable.Empty<int>() : new[] { match.HomeTeamId, match.AwayTeamId },
            cancellationToken);
        return MapToDto(match, teams);
    }

    public async Task<(bool Success, string? Error, MatchDto? Match)> ProgramMatchAsync(MatchProgramRequest request, CancellationToken cancellationToken = default)
    {
        if (request.HomeTeamId == request.AwayTeamId)
        {
            return (false, "El equipo local y visitante deben ser distintos", null);
        }

        var teamsExist = await _teamClient.TeamsExistAsync(request.HomeTeamId, request.AwayTeamId, cancellationToken);
        if (!teamsExist)
        {
            return (false, "Alguno de los equipos seleccionados no existe", null);
        }

        var scheduled = request.Date.ToDateTime(request.Time);
        var match = new Match
        {
            HomeTeamId = request.HomeTeamId,
            AwayTeamId = request.AwayTeamId,
            DateTime = DateTime.SpecifyKind(scheduled, DateTimeKind.Unspecified),
            Quarter = 1,
            TimeRemaining = request.QuarterDurationSeconds ?? DefaultQuarterSeconds,
            TimerRunning = false,
            Status = MatchStatus.Scheduled,
            FoulsHome = 0,
            FoulsAway = 0,
            HomeScore = 0,
            AwayScore = 0
        };

        await _repository.AddAsync(match);
        await _repository.SaveChangesAsync();

        var teams = await BuildTeamLookupAsync(new[] { match.HomeTeamId, match.AwayTeamId }, cancellationToken);
        var dto = MapToDto(match, teams);
        await BroadcastMatchUpdated(dto);
        return (true, null, dto);
    }

    public async Task<(bool Success, string? Error, MatchDto? Match)> UpdateScoreAsync(int matchId, ScoreRequest request, CancellationToken cancellationToken = default)
    {
        var match = await _repository.GetByIdAsync(matchId);
        if (match is null) return (false, "Partido no encontrado", null);
        if (string.Equals(match.Status, MatchStatus.Finished, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "El partido ya finalizó", null);
        }

        var isHome = IsHome(request.Team);
        if (!isHome.HasValue)
        {
            return (false, "Equipo inválido", null);
        }

        var delta = request.Points;
        if (isHome.Value)
        {
            match.HomeScore = Math.Max(0, match.HomeScore + delta);
        }
        else
        {
            match.AwayScore = Math.Max(0, match.AwayScore + delta);
        }

        if (match.HomeScore < 0) match.HomeScore = 0;
        if (match.AwayScore < 0) match.AwayScore = 0;

        if (!string.Equals(match.Status, MatchStatus.Live, StringComparison.OrdinalIgnoreCase))
        {
            match.Status = MatchStatus.Live;
        }

        await _repository.SaveChangesAsync();

        var teams = await BuildTeamLookupAsync(new[] { match.HomeTeamId, match.AwayTeamId }, cancellationToken);
        var dto = MapToDto(match, teams);

        await _hub.Clients.Group(MatchHub.GroupName(match.Id)).SendAsync("scoreUpdated", new
        {
            homeScore = match.HomeScore,
            awayScore = match.AwayScore
        }, cancellationToken);

        await BroadcastMatchUpdated(dto);
        return (true, null, dto);
    }

    public async Task<(bool Success, string? Error, MatchDto? Match)> RegisterFoulAsync(int matchId, FoulRequest request, CancellationToken cancellationToken = default)
    {
        var match = await _repository.GetByIdAsync(matchId);
        if (match is null) return (false, "Partido no encontrado", null);
        if (string.Equals(match.Status, MatchStatus.Finished, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "El partido ya finalizó", null);
        }

        var isHome = IsHome(request.Team);
        if (!isHome.HasValue)
        {
            return (false, "Equipo inválido", null);
        }

        var amount = request.Amount == 0 ? 1 : request.Amount;
        if (isHome.Value)
        {
            match.FoulsHome = Math.Max(0, match.FoulsHome + amount);
        }
        else
        {
            match.FoulsAway = Math.Max(0, match.FoulsAway + amount);
        }

        await _repository.SaveChangesAsync();

        var teams = await BuildTeamLookupAsync(new[] { match.HomeTeamId, match.AwayTeamId }, cancellationToken);
        var dto = MapToDto(match, teams);

        await _hub.Clients.Group(MatchHub.GroupName(match.Id)).SendAsync("foulsUpdated", new
        {
            homeFouls = match.FoulsHome,
            awayFouls = match.FoulsAway
        }, cancellationToken);

        await BroadcastMatchUpdated(dto);
        return (true, null, dto);
    }

    public async Task<(bool Success, string? Error, MatchDto? Match)> UpdateTimerAsync(int matchId, TimerRequest request, CancellationToken cancellationToken = default)
    {
        var match = await _repository.GetByIdAsync(matchId);
        if (match is null) return (false, "Partido no encontrado", null);

        var action = (request.Action ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(action))
        {
            return (false, "Acción de temporizador inválida", null);
        }

        if (string.Equals(match.Status, MatchStatus.Finished, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "El partido ya finalizó", null);
        }

        var timeRemaining = request.TimeRemaining ?? match.TimeRemaining;
        if (timeRemaining < 0) timeRemaining = 0;

        var events = new List<(string Name, object Payload)>();

        switch (action)
        {
            case "start":
                match.TimeRemaining = timeRemaining > 0 ? timeRemaining : DefaultQuarterSeconds;
                match.TimerRunning = true;
                match.Status = MatchStatus.Live;
                events.Add(("timerStarted", new
                {
                    remainingSeconds = match.TimeRemaining,
                    quarterEndsAtUtc = DateTime.UtcNow.AddSeconds(match.TimeRemaining)
                }));
                break;
            case "pause":
                match.TimeRemaining = timeRemaining;
                match.TimerRunning = false;
                events.Add(("timerPaused", new
                {
                    remainingSeconds = match.TimeRemaining
                }));
                break;
            case "resume":
                match.TimeRemaining = timeRemaining > 0 ? timeRemaining : match.TimeRemaining;
                match.TimerRunning = true;
                match.Status = MatchStatus.Live;
                events.Add(("timerResumed", new
                {
                    remainingSeconds = match.TimeRemaining,
                    quarterEndsAtUtc = DateTime.UtcNow.AddSeconds(match.TimeRemaining)
                }));
                break;
            case "reset":
                match.TimeRemaining = request.TimeRemaining ?? DefaultQuarterSeconds;
                match.TimerRunning = false;
                events.Add(("timerReset", new
                {
                    remainingSeconds = match.TimeRemaining
                }));
                break;
            case "set":
                match.TimeRemaining = timeRemaining;
                events.Add(("timerUpdated", new
                {
                    remainingSeconds = match.TimeRemaining
                }));
                break;
            default:
                return (false, "Acción no soportada", null);
        }

        await _repository.SaveChangesAsync();

        var teams = await BuildTeamLookupAsync(new[] { match.HomeTeamId, match.AwayTeamId }, cancellationToken);
        var dto = MapToDto(match, teams);
        foreach (var (name, payload) in events)
        {
            await _hub.Clients.Group(MatchHub.GroupName(match.Id)).SendAsync(name, payload, cancellationToken);
        }

        await BroadcastMatchUpdated(dto);
        return (true, null, dto);
    }

    public async Task<(bool Success, string? Error, MatchDto? Match)> AdvanceQuarterAsync(int matchId, CancellationToken cancellationToken = default)
    {
        var match = await _repository.GetByIdAsync(matchId);
        if (match is null) return (false, "Partido no encontrado", null);

        if (string.Equals(match.Status, MatchStatus.Finished, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "El partido ya finalizó", null);
        }

        match.Quarter = Math.Min(match.Quarter + 1, 4);
        match.TimeRemaining = DefaultQuarterSeconds;
        match.TimerRunning = false;

        await _repository.SaveChangesAsync();

        var teams = await BuildTeamLookupAsync(new[] { match.HomeTeamId, match.AwayTeamId }, cancellationToken);
        var dto = MapToDto(match, teams);

        await _hub.Clients.Group(MatchHub.GroupName(match.Id)).SendAsync("quarterChanged", new
        {
            quarter = match.Quarter
        }, cancellationToken);

        await _hub.Clients.Group(MatchHub.GroupName(match.Id)).SendAsync("timerReset", new
        {
            remainingSeconds = match.TimeRemaining
        }, cancellationToken);

        await BroadcastMatchUpdated(dto);
        return (true, null, dto);
    }

    public async Task<(bool Success, string? Error, MatchDto? Match)> FinishMatchAsync(int matchId, FinishMatchRequest request, CancellationToken cancellationToken = default)
    {
        var match = await _repository.GetByIdAsync(matchId);
        if (match is null) return (false, "Partido no encontrado", null);

        if (request.HomeScore.HasValue) match.HomeScore = Math.Max(0, request.HomeScore.Value);
        if (request.AwayScore.HasValue) match.AwayScore = Math.Max(0, request.AwayScore.Value);

        match.Status = MatchStatus.Finished;
        match.TimerRunning = false;
        match.TimeRemaining = 0;

        await _repository.SaveChangesAsync();

        var teams = await BuildTeamLookupAsync(new[] { match.HomeTeamId, match.AwayTeamId }, cancellationToken);
        var dto = MapToDto(match, teams);

        await _hub.Clients.Group(MatchHub.GroupName(match.Id)).SendAsync("scoreUpdated", new
        {
            homeScore = match.HomeScore,
            awayScore = match.AwayScore
        }, cancellationToken);

        var winner = match.HomeScore == match.AwayScore
            ? "draw"
            : match.HomeScore > match.AwayScore ? "home" : "away";

        await _hub.Clients.Group(MatchHub.GroupName(match.Id)).SendAsync("gameEnded", new
        {
            home = match.HomeScore,
            away = match.AwayScore,
            winner
        }, cancellationToken);

        await BroadcastMatchUpdated(dto);
        return (true, null, dto);
    }

    private async Task BroadcastMatchUpdated(MatchDto dto)
    {
        await _hub.Clients.All.SendAsync("matchUpdated", dto);
    }

    private static bool? IsHome(string? team)
    {
        if (string.IsNullOrWhiteSpace(team)) return null;
        var normalized = team.Trim().ToLowerInvariant();
        return normalized switch
        {
            "home" => true,
            "local" => true,
            "away" => false,
            "visitante" => false,
            _ => null
        };
    }

    private async Task<Dictionary<int, string>> BuildTeamLookupAsync(
        IEnumerable<int> requiredIds,
        CancellationToken cancellationToken)
    {
        var lookup = new Dictionary<int, string>();
        try
        {
            var teams = await _teamClient.GetTeamsAsync(cancellationToken);
            foreach (var team in teams)
            {
                if (!lookup.ContainsKey(team.Id))
                {
                    lookup[team.Id] = team.Name;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo obtener la lista de equipos para enriquecer los partidos");
        }

        var missingIds = (requiredIds ?? Array.Empty<int>())
            .Where(id => id > 0 && !lookup.ContainsKey(id))
            .Distinct()
            .ToList();

        foreach (var id in missingIds)
        {
            try
            {
                var team = await _teamClient.GetTeamAsync(id, cancellationToken);
                if (team is not null && !lookup.ContainsKey(id))
                {
                    lookup[id] = team.Name;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener el equipo {TeamId}", id);
            }
        }

        return lookup;
    }

    private static MatchDto MapToDto(Match match, IReadOnlyDictionary<int, string> teams)
    {
        teams.TryGetValue(match.HomeTeamId, out var homeName);
        teams.TryGetValue(match.AwayTeamId, out var awayName);

        return new MatchDto
        {
            Id = match.Id,
            HomeTeamId = match.HomeTeamId,
            AwayTeamId = match.AwayTeamId,
            HomeTeamName = homeName ?? string.Empty,
            AwayTeamName = awayName ?? string.Empty,
            HomeScore = match.HomeScore,
            AwayScore = match.AwayScore,
            FoulsHome = match.FoulsHome,
            FoulsAway = match.FoulsAway,
            Quarter = match.Quarter,
            TimeRemaining = match.TimeRemaining,
            TimerRunning = match.TimerRunning,
            Status = match.Status,
            DateTime = match.DateTime
        };
    }
}
