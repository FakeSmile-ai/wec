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

public interface ITeamClientService
{
    Task<IReadOnlyList<TeamSummary>> GetTeamsAsync(CancellationToken cancellationToken = default);
    Task<TeamSummary?> GetTeamAsync(int teamId, CancellationToken cancellationToken = default);
    Task<bool> TeamsExistAsync(int homeTeamId, int awayTeamId, CancellationToken cancellationToken = default);
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
            using var response = await _httpClient.GetAsync($"{_baseUrl}/{teamId}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Team {TeamId} not found. Status: {StatusCode}", teamId, response.StatusCode);
                return null;
            }

            var team = await response.Content.ReadFromJsonAsync<TeamSummary>(cancellationToken: cancellationToken);
            if (team is null)
            {
                var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                var parsed = ParseSingle(document.RootElement);
                return parsed;
            }

            return team;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving team {TeamId}", teamId);
            return null;
        }
    }

    public async Task<bool> TeamsExistAsync(int homeTeamId, int awayTeamId, CancellationToken cancellationToken = default)
    {
        if (homeTeamId <= 0 || awayTeamId <= 0 || homeTeamId == awayTeamId) return false;

        var tasks = new[]
        {
            GetTeamAsync(homeTeamId, cancellationToken),
            GetTeamAsync(awayTeamId, cancellationToken)
        };

        await Task.WhenAll(tasks);
        return tasks[0].Result is not null && tasks[1].Result is not null;
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
