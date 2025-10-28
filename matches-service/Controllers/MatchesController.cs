using Microsoft.AspNetCore.Mvc;
using MatchesService.Models.DTOs;
using MatchesService.Services;

namespace MatchesService.Controllers;

[ApiController]
[Route("api/matches")]
[Produces("application/json")]
public class MatchesController : ControllerBase
{
    private readonly IMatchService _matchService;

    public MatchesController(IMatchService matchService)
    {
        _matchService = matchService;
    }

    [HttpGet("health")]
    public IActionResult Health() => Ok("OK");

    [HttpGet]
    public async Task<IActionResult> GetMatches(CancellationToken cancellationToken)
    {
        var matches = await _matchService.GetMatchesAsync(cancellationToken);
        return Ok(matches);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMatch(int id, CancellationToken cancellationToken)
    {
        var match = await _matchService.GetMatchAsync(id, cancellationToken);
        return match is null ? NotFound(new { error = "Partido no encontrado" }) : Ok(match);
    }

    [HttpPost("programar")]
    public async Task<IActionResult> ProgramMatch([FromBody] MatchProgramRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _matchService.ProgramMatchAsync(request, cancellationToken);
        return result.Success ? Ok(result.Match) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{matchId:int}/score")]
    public async Task<IActionResult> UpdateScore(int matchId, [FromBody] ScoreRequest request, CancellationToken cancellationToken)
    {
        var result = await _matchService.UpdateScoreAsync(matchId, request, cancellationToken);
        return result.Success ? Ok(result.Match) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{matchId:int}/foul")]
    public async Task<IActionResult> RegisterFoul(int matchId, [FromBody] FoulRequest request, CancellationToken cancellationToken)
    {
        var result = await _matchService.RegisterFoulAsync(matchId, request, cancellationToken);
        return result.Success ? Ok(result.Match) : BadRequest(new { error = result.Error });
    }

    [HttpPatch("{matchId:int}/timer")]
    public async Task<IActionResult> UpdateTimer(int matchId, [FromBody] TimerRequest request, CancellationToken cancellationToken)
    {
        var result = await _matchService.UpdateTimerAsync(matchId, request, cancellationToken);
        return result.Success ? Ok(result.Match) : BadRequest(new { error = result.Error });
    }

    [HttpPatch("{matchId:int}/quarter")]
    public async Task<IActionResult> AdvanceQuarter(int matchId, CancellationToken cancellationToken)
    {
        var result = await _matchService.AdvanceQuarterAsync(matchId, cancellationToken);
        return result.Success ? Ok(result.Match) : BadRequest(new { error = result.Error });
    }

    [HttpPatch("{matchId:int}/finish")]
    public async Task<IActionResult> FinishMatch(int matchId, [FromBody] FinishMatchRequest request, CancellationToken cancellationToken)
    {
        var result = await _matchService.FinishMatchAsync(matchId, request, cancellationToken);
        return result.Success ? Ok(result.Match) : BadRequest(new { error = result.Error });
    }
}
