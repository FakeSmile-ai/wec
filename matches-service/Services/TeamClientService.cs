using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MatchesService.Services;

public record TeamSummary(int Id, string Name);

public class TeamsServiceOptions
{
    public string BaseUrl { get; set; } = "http://teams-service:8082/api/teams";
}

public record TeamValidationResult(bool IsValid, bool ServiceAvailable, TeamSummary? HomeTeam, TeamSummary? AwayTeam)
{
    public bool HomeExists => HomeTeam is not null;
    public bool AwayExists => AwayTeam is not null;
}

public interface ITeamClientService
{
    Task<IReadOnlyList<TeamSummary>> GetTeamsAsync(CancellationToken cancellationToken = default);
    Task<TeamSummary?> GetTeamAsync(int teamId, CancellationToken cancellationToken = default);
    Task<TeamValidationResult> ValidateTeamsAsync(int homeTeamId, int awayTeamId, CancellationToken cancellationToken = default);
}

public class TeamClientService : ITeamClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TeamClientService> _logger;
    private readonly string _baseUrl;

    public TeamClientService(HttpClient httpClient, IOptions<TeamsServiceOptions> options, ILogger<TeamClientService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = (options.Value.BaseUrl ?? string.Empty).Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(_baseUrl))
        {
            _baseUrl = "http://teams-service:8082/api/teams";
        }
    }

    public async Task<IReadOnlyList<TeamSummary>> GetTeamsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(_baseUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Teams service returned status {StatusCode}", response.StatusCode);
                return Array.Empty<TeamSummary>();
            }

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return ParseTeams(document.RootElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teams from {Url}", _baseUrl);
            return Array.Empty<TeamSummary>();
        }
    }

    public async Task<TeamSummary?> GetTeamAsync(int teamId, CancellationToken cancellationToken = default)
    {
        if (teamId <= 0) return null;

        try
        {
            return await FetchTeamAsync(teamId, cancellationToken);
        }
        catch (TeamsServiceUnavailableException ex)
        {
            _logger.LogError(ex, "Teams service unavailable when retrieving team {TeamId}", teamId);
            return null;
        }
    }

    public async Task<TeamValidationResult> ValidateTeamsAsync(int homeTeamId, int awayTeamId, CancellationToken cancellationToken = default)
    {
        if (homeTeamId <= 0 || awayTeamId <= 0 || homeTeamId == awayTeamId)
        {
            return new TeamValidationResult(false, true, null, null);
        }

        try
        {
            var homeTask = FetchTeamAsync(homeTeamId, cancellationToken);
            var awayTask = FetchTeamAsync(awayTeamId, cancellationToken);

            await Task.WhenAll(homeTask, awayTask);

            var home = await homeTask;
            var away = await awayTask;

            if (home is null || away is null)
            {
                return new TeamValidationResult(false, true, home, away);
            }

            return new TeamValidationResult(true, true, home, away);
        }
        catch (TeamsServiceUnavailableException ex)
        {
            _logger.LogError(ex, "Teams service unavailable while validating teams {HomeTeamId} vs {AwayTeamId}", homeTeamId, awayTeamId);
            return new TeamValidationResult(false, false, null, null);
        }
    }

    private async Task<TeamSummary?> FetchTeamAsync(int teamId, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync($"{_baseUrl}/{teamId}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Team {TeamId} not found in teams-service", teamId);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new TeamsServiceUnavailableException($"Teams service returned status {(int)response.StatusCode} ({response.StatusCode})");
            }

            var team = await response.Content.ReadFromJsonAsync<TeamSummary>(cancellationToken: cancellationToken);
            if (team is not null)
            {
                return team;
            }

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return ParseSingle(document.RootElement);
        }
        catch (TeamsServiceUnavailableException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new TeamsServiceUnavailableException("Error connecting to teams-service", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving team {TeamId}", teamId);
            return null;
        }
    }

    private static IReadOnlyList<TeamSummary> ParseTeams(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            return root.Deserialize<List<TeamSummary>>() ?? new List<TeamSummary>();
        }

        if (root.ValueKind == JsonValueKind.Object)
        {
            if (root.TryGetProperty("content", out var content))
            {
                return ParseTeams(content);
            }

            if (root.TryGetProperty("data", out var data))
            {
                return ParseTeams(data);
            }

            if (root.TryGetProperty("teams", out var teams))
            {
                return ParseTeams(teams);
            }

            var single = ParseSingle(root);
            if (single is not null)
            {
                return new List<TeamSummary> { single };
            }
        }

        return Array.Empty<TeamSummary>();
    }

    private static TeamSummary? ParseSingle(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object) return null;

        if (element.TryGetProperty("id", out var idProperty) && idProperty.TryGetInt32(out var id))
        {
            string name = element.TryGetProperty("name", out var nameProperty)
                ? nameProperty.GetString() ?? string.Empty
                : string.Empty;
            return new TeamSummary(id, name);
        }

        return null;
    }
}

public sealed class TeamsServiceUnavailableException : Exception
{
    public TeamsServiceUnavailableException(string message) : base(message) { }
    public TeamsServiceUnavailableException(string message, Exception innerException) : base(message, innerException) { }
}
