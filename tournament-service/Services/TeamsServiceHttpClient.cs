using System.Text.Json;
using System.Text.Json.Serialization;
using TournamentService.Models;

namespace TournamentService.Services; // <-- El namespace correcto

public class TeamsServiceHttpClient
{
    private readonly HttpClient _httpClient;

    public TeamsServiceHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Team>?> GetTeamsAsync()
    {
        try
        {
            var responseStream = await _httpClient.GetStreamAsync("api/teams");
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var pagedResponse = await JsonSerializer.DeserializeAsync<PagedTeamsResponse>(responseStream, options);

            return pagedResponse?.Content;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error al conectar con Teams Service: {ex.Message}");
            return null;
        }
    }
}

public class PagedTeamsResponse
{
    [JsonPropertyName("content")]
    public List<Team>? Content { get; set; }
}