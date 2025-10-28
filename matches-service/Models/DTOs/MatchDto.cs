namespace MatchesService.Models.DTOs;

public class MatchDto
{
    public int Id { get; set; }
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public string HomeTeamName { get; set; } = string.Empty;
    public string AwayTeamName { get; set; } = string.Empty;
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public int FoulsHome { get; set; }
    public int FoulsAway { get; set; }
    public int Quarter { get; set; }
    public int TimeRemaining { get; set; }
    public bool TimerRunning { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
}
