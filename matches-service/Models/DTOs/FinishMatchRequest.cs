using System.ComponentModel.DataAnnotations;

namespace MatchesService.Models.DTOs;

public class FinishMatchRequest
{
    [Range(0, int.MaxValue)]
    public int? HomeScore { get; set; }

    [Range(0, int.MaxValue)]
    public int? AwayScore { get; set; }
}
