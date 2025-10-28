using System.ComponentModel.DataAnnotations;

namespace MatchesService.Models.DTOs;

public class TimerRequest
{
    [Required]
    public string Action { get; set; } = string.Empty; // start | pause | resume | reset | set

    [Range(0, 7200)]
    public int? TimeRemaining { get; set; }
}
