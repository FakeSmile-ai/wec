using Microsoft.AspNetCore.Mvc;
using TournamentService.Models;
using TournamentService.Services; // <-- La directiva using que faltaba o era incorrecta

namespace TournamentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TournamentController : ControllerBase
{
    private readonly TeamsServiceHttpClient _teamsServiceClient;

    public TournamentController(TeamsServiceHttpClient teamsServiceClient)
    {
        _teamsServiceClient = teamsServiceClient;
    }

    [HttpGet("generate")]
    public async Task<ActionResult<Tournament>> GenerateTournament([FromQuery] int numberOfTeams = 4)
    {
        if (numberOfTeams < 4 || (numberOfTeams & (numberOfTeams - 1)) != 0)
        {
            return BadRequest("El número de equipos debe ser una potencia de 2 (4, 8, 16, etc.).");
        }

        var allTeams = await _teamsServiceClient.GetTeamsAsync();

        if (allTeams == null || allTeams.Count < numberOfTeams)
        {
            return BadRequest($"No se pudieron obtener suficientes equipos. Se necesitan {numberOfTeams} y solo se encontraron {allTeams?.Count ?? 0}.");
        }
        
        var teamsForTournament = allTeams.Take(numberOfTeams).ToList();
        var shuffledTeams = teamsForTournament.OrderBy(a => Guid.NewGuid()).ToList();

        var tournament = new Tournament
        {
            TournamentName = $"Torneo de {numberOfTeams} equipos"
        };
        
        int matchIdCounter = 1;
        
        for (int i = 0; i < shuffledTeams.Count; i += 2)
        {
            var match = new Match
            {
                MatchId = matchIdCounter++,
                Round = 1,
                TeamA = shuffledTeams[i],
                TeamB = shuffledTeams[i + 1]
            };
            tournament.Matches.Add(match);
        }

        return Ok(tournament);
    }
}