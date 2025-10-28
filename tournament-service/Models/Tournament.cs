namespace TournamentService.Models;

public class Tournament
{
    public string TournamentName { get; set; } = "Mi Torneo";
    public List<Match> Matches { get; set; } = new List<Match>();
}