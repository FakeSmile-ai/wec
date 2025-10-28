using System.ComponentModel.DataAnnotations;

namespace MatchesService.Models.DTOs;

public class MatchProgramRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int HomeTeamId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int AwayTeamId { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public TimeOnly Time { get; set; }

    public int? QuarterDurationSeconds { get; set; }
}
