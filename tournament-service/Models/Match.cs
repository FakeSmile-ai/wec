namespace TournamentService.Models;

public class Match
{
    public int MatchId { get; set; }
    public int Round { get; set; }
    public Team? TeamA { get; set; }
    public Team? TeamB { get; set; }
    public int? ScoreA { get; set; }
    public int? ScoreB { get; set; }
    public int? WinnerId { get; set; } // El ID del equipo ganador
}