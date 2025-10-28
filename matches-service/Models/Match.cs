using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatchesService.Models;

public static class MatchStatus
{
    public const string Scheduled = "Scheduled";
    public const string Live = "Live";
    public const string Finished = "Finished";
}

public class Match
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int HomeTeamId { get; set; }

    [Range(1, int.MaxValue)]
    public int AwayTeamId { get; set; }

    [Range(0, int.MaxValue)]
    public int HomeScore { get; set; }

    [Range(0, int.MaxValue)]
    public int AwayScore { get; set; }

    [Range(0, int.MaxValue)]
    public int FoulsHome { get; set; }

    [Range(0, int.MaxValue)]
    public int FoulsAway { get; set; }

    [Range(1, 4)]
    public int Quarter { get; set; } = 1;

    [Range(0, int.MaxValue)]
    public int TimeRemaining { get; set; } = 600;

    public bool TimerRunning { get; set; }

    [Required, MaxLength(32)]
    public string Status { get; set; } = MatchStatus.Scheduled;

    [Column("DateMatch")]
    public DateTime DateTime { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
