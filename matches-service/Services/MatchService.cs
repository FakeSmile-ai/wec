using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MatchesService.Data;              // ⬅️ FALTABA ESTE using (DbContext)
using MatchesService.Hubs;
using MatchesService.Models;
using MatchesService.Models.DTOs;
using MatchesService.Repositories;
using MatchesService.Services.Runtime;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;

namespace MatchesService.Services
{
    /// <summary>
    /// Lógica de negocio de partidos. Integra teams-service vía HTTP.
    /// </summary>
    public class MatchService : IMatchService
    {
        private readonly IMatchRepository _repo;
        private readonly IMatchRunTime _runTime;
        private readonly IHubContext<ScoreHub> _hub;
        private readonly HttpClient _http;
        private readonly string _teamsApiBase;
        private readonly MatchesDbContext _db; // DbContext inyectado para transacciones

        public MatchService(
            IMatchRepository repo,
            IMatchRunTime runTime,
            IHubContext<ScoreHub> hub,
            IHttpClientFactory httpFactory,
            IConfiguration config,
            MatchesDbContext db)
        {
            _repo = repo;
            _runTime = runTime;
            _hub = hub;
            _http = httpFactory.CreateClient();
            _db = db;

            _teamsApiBase = (config["TeamsService:BaseUrl"] ?? "http://teams-service:8082/api/teams").TrimEnd('/');
            if (!_teamsApiBase.EndsWith("/teams", StringComparison.OrdinalIgnoreCase))
                _teamsApiBase += "/teams";
        }

        // ==================== CONSULTAS ====================
        public async Task<object> ListarAsync(int page, int pageSize, string? status, int? teamId, DateTime? from, DateTime? to)
        {
            var items = await _repo.GetAllAsync(page, pageSize, status, teamId, from, to);
            var total = await _repo.CountAsync(status, teamId, from, to);
            return new { Total = total, Data = items };
        }

        public async Task<object?> GetMatchAsync(int id)
        {
            var match = await _repo.GetByIdAsync(id);
            if (match == null) return null;

            JObject? homeTeam = null;
            JObject? awayTeam = null;

            try
            {
                var homeResp = await _http.GetAsync($"{_teamsApiBase}/{match.HomeTeamId}");
                if (homeResp.IsSuccessStatusCode)
                {
                    var homeJson = await homeResp.Content.ReadAsStringAsync();
                    homeTeam = JObject.Parse(homeJson);
                }

                var awayResp = await _http.GetAsync($"{_teamsApiBase}/{match.AwayTeamId}");
                if (awayResp.IsSuccessStatusCode)
                {
                    var awayJson = await awayResp.Content.ReadAsStringAsync();
                    awayTeam = JObject.Parse(awayJson);
                }
            }
            catch { /* enriquecimiento best-effort */ }

            return new
            {
                match.Id,
                match.Status,
                match.DateMatch,
                match.HomeTeamId,
                match.AwayTeamId,
                match.HomeScore,
                match.AwayScore,
                match.Period,
                HomeTeam = homeTeam,
                AwayTeam = awayTeam
            };
        }

        public async Task<object> ProximosAsync()
        {
            var data = await _repo.GetUpcomingAsync();
            return new { Data = data };
        }

        public async Task<(bool Success, string? Error, object? Data)> RangoAsync(DateTime from, DateTime to)
        {
            var data = await _repo.GetByRangeAsync(from, to);
            return (true, null, data);
        }

        // ==================== PROGRAMAR / REPROGRAMAR ====================
        public async Task<(bool Success, string? Error, object? Data)> ProgramarAsync(ProgramarPartidoDto dto)
        {
            try
            {
                var homeExists = await _http.GetAsync($"{_teamsApiBase}/{dto.HomeTeamId}");
                var awayExists = await _http.GetAsync($"{_teamsApiBase}/{dto.AwayTeamId}");
                if (!homeExists.IsSuccessStatusCode || !awayExists.IsSuccessStatusCode)
                    return (false, "Uno o ambos equipos no existen en teams-service", null);

                var match = new Match
                {
                    HomeTeamId = dto.HomeTeamId,
                    AwayTeamId = dto.AwayTeamId,
                    DateMatch = dto.DateMatch,
                    QuarterDurationSeconds = dto.QuarterDurationSeconds ?? 600,
                    Status = "Scheduled"
                };

                await _repo.AddAsync(match);
                await _repo.SaveChangesAsync();
                return (true, null, match);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string? Error, object? Data)> ReprogramarAsync(int id, ReprogramarDto dto)
        {
            var match = await _repo.GetByIdAsync(id);
            if (match == null) return (false, "Partido no encontrado", null);

            match.DateMatch = dto.NewDate;
            match.Status = "Rescheduled";

            await _repo.UpdateAsync(match);
            await _repo.SaveChangesAsync();

            return (true, null, match);
        }

        // ==================== CREAR RÁPIDO ====================
        public async Task<(bool Success, string? Error, object? Data)> NewGameAsync(NewGameDto dto)
        {
            // Nota: tu teams-service podría requerir más campos (coach/city)
            var home = new { name = dto.HomeName };
            var away = new { name = dto.AwayName };

            var homeRes = await _http.PostAsJsonAsync(_teamsApiBase, home);
            var awayRes = await _http.PostAsJsonAsync(_teamsApiBase, away);

            if (!homeRes.IsSuccessStatusCode || !awayRes.IsSuccessStatusCode)
                return (false, "Error creando equipos en teams-service", null);

            var homeTeam = await homeRes.Content.ReadFromJsonAsync<TeamResponse>();
            var awayTeam = await awayRes.Content.ReadFromJsonAsync<TeamResponse>();

            var match = new Match
            {
                HomeTeamId = homeTeam!.Id,
                AwayTeamId = awayTeam!.Id,
                QuarterDurationSeconds = dto.QuarterDurationSeconds ?? 600,
                Status = "Scheduled"
            };

            await _repo.AddAsync(match);
            await _repo.SaveChangesAsync();
            return (true, null, match);
        }

        public async Task<(bool Success, string? Error, object? Data)> NewByTeamsAsync(NewGameByTeamsDto dto)
        {
            var homeExists = await _http.GetAsync($"{_teamsApiBase}/{dto.HomeTeamId}");
            var awayExists = await _http.GetAsync($"{_teamsApiBase}/{dto.AwayTeamId}");
            if (!homeExists.IsSuccessStatusCode || !awayExists.IsSuccessStatusCode)
                return (false, "Uno o ambos equipos no existen en teams-service", null);

            var match = new Match
            {
                HomeTeamId = dto.HomeTeamId,
                AwayTeamId = dto.AwayTeamId,
                QuarterDurationSeconds = dto.QuarterDurationSeconds ?? 600,
                Status = "Scheduled"
            };

            await _repo.AddAsync(match);
            await _repo.SaveChangesAsync();
            return (true, null, match);
        }

        // ==================== SCORE ====================
        public async Task<(bool Success, string? Error, object? Data)> AddScoreAsync(int id, AddScoreDto dto)
        {
            var match = await _repo.GetByIdAsync(id);
            if (match is null) return (false, "Partido no encontrado", null);

            if (dto.TeamId != match.HomeTeamId && dto.TeamId != match.AwayTeamId)
                return (false, "El TeamId no pertenece a este partido", null);

            if (dto.Points < -3 || dto.Points > 3)
                return (false, "Points debe estar entre -3 y 3", null);

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                if (dto.TeamId == match.HomeTeamId)
                    match.HomeScore = Math.Max(0, match.HomeScore + dto.Points);
                else
                    match.AwayScore = Math.Max(0, match.AwayScore + dto.Points);

                var ev = new ScoreEvent
                {
                    MatchId = match.Id,
                    TeamId = dto.TeamId,
                    PlayerId = dto.PlayerId,
                    Points = dto.Points,
                    DateRegister = DateTime.UtcNow,
                    Note = null
                };

                await _repo.AddScoreEventAsync(ev);
                await _repo.UpdateAsync(match);
                await _repo.SaveChangesAsync();
                await tx.CommitAsync();

                var payload = new
                {
                    matchId = match.Id,
                    homeScore = match.HomeScore,
                    awayScore = match.AwayScore,
                    @event = new { ev.Id, ev.TeamId, ev.PlayerId, ev.Points, ev.DateRegister }
                };

                return (true, null, payload);
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                return (false, ex.InnerException?.Message ?? ex.Message, null);
            }
        }

        public async Task<(bool Success, string? Error, object? Data)> AdjustScoreAsync(int id, AdjustScoreDto dto)
        {
            var match = await _repo.GetByIdAsync(id);
            if (match is null) return (false, "Partido no encontrado", null);

            if (dto.TeamId != match.HomeTeamId && dto.TeamId != match.AwayTeamId)
                return (false, "El TeamId no pertenece a este partido", null);

            if (dto.Delta == 0)
                return (true, null, new { matchId = id, match.HomeScore, match.AwayScore });

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                int remaining = dto.Delta;
                var now = DateTime.UtcNow;
                var created = new List<object>();

                while (remaining != 0)
                {
                    int step = Math.Clamp(remaining, -3, 3);

                    if (dto.TeamId == match.HomeTeamId)
                        match.HomeScore = Math.Max(0, match.HomeScore + step);
                    else
                        match.AwayScore = Math.Max(0, match.AwayScore + step);

                    var ev = new ScoreEvent
                    {
                        MatchId = match.Id,
                        TeamId = dto.TeamId,
                        PlayerId = null,
                        Points = step,
                        Note = "adjust",
                        DateRegister = now
                    };
                    await _repo.AddScoreEventAsync(ev);
                    created.Add(new { ev.TeamId, ev.Points, ev.DateRegister, ev.Note });

                    remaining -= step;
                }

                await _repo.UpdateAsync(match);
                await _repo.SaveChangesAsync();
                await tx.CommitAsync();

                var payload = new
                {
                    matchId = match.Id,
                    homeScore = match.HomeScore,
                    awayScore = match.AwayScore,
                    adjustments = created
                };

                return (true, null, payload);
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                return (false, ex.InnerException?.Message ?? ex.Message, null);
            }
        }

        // ==================== FOULS ====================
        public async Task<(bool Success, string? Error, object? Data)> AddFoulAsync(int id, AddFoulDto dto)
        {
            var match = await _repo.GetByIdAsync(id);
            if (match is null) return (false, "Partido no encontrado", null);

            if (dto.TeamId != match.HomeTeamId && dto.TeamId != match.AwayTeamId)
                return (false, "El TeamId no pertenece a este partido", null);

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var foul = new Foul
                {
                    MatchId = match.Id,
                    TeamId = dto.TeamId,
                    PlayerId = dto.PlayerId,
                    Type = string.IsNullOrWhiteSpace(dto.Type) ? null : dto.Type!.Trim(),
                    DateRegister = DateTime.UtcNow
                };

                await _repo.AddFoulAsync(foul);
                await _repo.SaveChangesAsync();
                await tx.CommitAsync();

                var homeFouls = await _repo.GetFoulCountAsync(match.Id, match.HomeTeamId);
                var awayFouls = await _repo.GetFoulCountAsync(match.Id, match.AwayTeamId);

                var payload = new
                {
                    matchId = match.Id,
                    foul = new { foul.Id, foul.TeamId, foul.PlayerId, foul.Type, foul.DateRegister },
                    totals = new { homeTeamId = match.HomeTeamId, homeFouls, awayTeamId = match.AwayTeamId, awayFouls }
                };

                return (true, null, payload);
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                return (false, ex.InnerException?.Message ?? ex.Message, null);
            }
        }

        public async Task<(bool Success, string? Error, object? Data)> AdjustFoulAsync(int id, AdjustFoulDto dto)
        {
            var match = await _repo.GetByIdAsync(id);
            if (match is null) return (false, "Partido no encontrado", null);

            if (dto.TeamId != match.HomeTeamId && dto.TeamId != match.AwayTeamId)
                return (false, "El TeamId no pertenece a este partido", null);

            if (dto.Delta == 0)
            {
                var homeF = await _repo.GetFoulCountAsync(match.Id, match.HomeTeamId);
                var awayF = await _repo.GetFoulCountAsync(match.Id, match.AwayTeamId);
                return (true, null, new { matchId = match.Id, totals = new { homeFouls = homeF, awayFouls = awayF } });
            }

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var created = 0;
                var removed = 0;

                if (dto.Delta > 0)
                {
                    var now = DateTime.UtcNow;
                    for (int i = 0; i < dto.Delta; i++)
                    {
                        var foul = new Foul
                        {
                            MatchId = match.Id,
                            TeamId = dto.TeamId,
                            PlayerId = null,
                            Type = "Adjust",
                            DateRegister = now
                        };
                        await _repo.AddFoulAsync(foul);
                        created++;
                    }
                }
                else // dto.Delta < 0
                {
                    removed = await _repo.RemoveLastFoulsAsync(match.Id, dto.TeamId, Math.Abs(dto.Delta));
                }

                await _repo.SaveChangesAsync();
                await tx.CommitAsync();

                var homeFouls = await _repo.GetFoulCountAsync(match.Id, match.HomeTeamId);
                var awayFouls = await _repo.GetFoulCountAsync(match.Id, match.AwayTeamId);

                var payload = new
                {
                    matchId = match.Id,
                    teamId = dto.TeamId,
                    created,
                    removed,
                    totals = new { homeTeamId = match.HomeTeamId, homeFouls, awayTeamId = match.AwayTeamId, awayFouls }
                };

                return (true, null, payload);
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                return (false, ex.InnerException?.Message ?? ex.Message, null);
            }
        }

        // ==================== TIMER / PERIODOS / FIN ====================
        public Task<(bool Success, string? Error, object? Data)> StartTimerAsync(int id, StartTimerDto? dto)
            => Task.FromResult<(bool, string?, object?)>((true, null, null));

        public Task<(bool Success, string? Error, object? Data)> PauseTimerAsync(int id)
            => Task.FromResult<(bool, string?, object?)>((true, null, null));

        public Task<(bool Success, string? Error, object? Data)> ResumeTimerAsync(int id)
            => Task.FromResult<(bool, string?, object?)>((true, null, null));

        public Task<(bool Success, string? Error, object? Data)> ResetTimerAsync(int id)
            => Task.FromResult<(bool, string?, object?)>((true, null, null));

        public Task<(bool Success, string? Error, object? Data, object? GameEnded)> AdvanceQuarterAsync(int id)
            => Task.FromResult<(bool, string?, object?, object?)>((true, null, null, null));

        public Task<(bool Success, string? Error, object? Data, object? GameEnded)> AutoAdvanceQuarterAsync(int id)
            => Task.FromResult<(bool, string?, object?, object?)>((true, null, null, null));

        public async Task<(bool Success, string? Error, object? Data, object? GameEnded)> FinishAsync(int id, FinishMatchDto dto)
        {
            // Cargar el partido
            var match = await _repo.GetByIdAsync(id);
            if (match is null)
                return (false, "Partido no encontrado", null, null);

            if (match.Status is "Finished" or "Canceled" or "Suspended")
                return (false, $"No se puede finalizar un partido en estado '{match.Status}'", null, null);

            if (dto.HomeScore < 0 || dto.AwayScore < 0)
                return (false, "Los puntajes no pueden ser negativos", null, null);

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // (Opcional) Registrar eventos finales enviados en el body
                if (dto.ScoreEvents is not null)
                {
                    foreach (var ev in dto.ScoreEvents)
                    {
                        if (ev.TeamId != match.HomeTeamId && ev.TeamId != match.AwayTeamId) continue;

                        _db.ScoreEvents.Add(new ScoreEvent
                        {
                            MatchId = match.Id,
                            TeamId = ev.TeamId,
                            PlayerId = ev.PlayerId,
                            Points = ev.Points,
                            DateRegister = ev.DateRegister
                        });
                    }
                }

                // (Opcional) Registrar faltas finales enviadas en el body
                if (dto.Fouls is not null)
                {
                    foreach (var f in dto.Fouls)
                    {
                        if (f.TeamId != match.HomeTeamId && f.TeamId != match.AwayTeamId) continue;

                        _db.Fouls.Add(new Foul
                        {
                            MatchId = match.Id,
                            TeamId = f.TeamId,
                            PlayerId = f.PlayerId,
                            Type = null,
                            DateRegister = f.DateRegister
                        });
                    }
                }

                // Actualizar marcadores y estado
                match.HomeScore = dto.HomeScore;
                match.AwayScore = dto.AwayScore;
                match.Status = "Finished";

                await _repo.UpdateAsync(match);
                await _repo.SaveChangesAsync();

                // Ganador (o empate)
                int? winnerTeamId = null;
                if (match.HomeScore > match.AwayScore) winnerTeamId = match.HomeTeamId;
                else if (match.AwayScore > match.HomeScore) winnerTeamId = match.AwayTeamId;

                if (winnerTeamId.HasValue)
                {
                    _db.TeamWins.Add(new TeamWin
                    {
                        MatchId = match.Id,
                        TeamId = winnerTeamId.Value,
                        DateRegistered = DateTime.UtcNow
                    });
                    await _db.SaveChangesAsync();
                }

                await tx.CommitAsync();

                var payload = new
                {
                    id = match.Id,
                    status = match.Status,
                    homeScore = match.HomeScore,
                    awayScore = match.AwayScore,
                    winnerTeamId = winnerTeamId
                };

                // Para enviar al Hub y como respuesta HTTP
                return (true, null, payload, payload);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, ex.Message, null, null);
            }
        }


        public Task<(bool Success, string? Error, object? Data)> CancelAsync(int id)
            => Task.FromResult<(bool, string?, object?)>((true, null, null));

        public Task<(bool Success, string? Error, object? Data)> SuspendAsync(int id)
            => Task.FromResult<(bool, string?, object?)>((true, null, null));
    }

    // DTO simple para leer respuesta de creación de equipo (id, name, etc.).
    public class TeamResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Color { get; set; }
        public string? Coach { get; set; }
        public string? City { get; set; }
    }
}
