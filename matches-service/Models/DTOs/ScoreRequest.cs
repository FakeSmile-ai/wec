using System.ComponentModel.DataAnnotations;

namespace MatchesService.Models.DTOs;

public class ScoreRequest
{
    [Required]
    public string Team { get; set; } = string.Empty; // "home" | "away"

    [Required]
    [Range(-4, 4)]
    public int Points { get; set; }
}
