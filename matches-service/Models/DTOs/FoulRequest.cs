using System.ComponentModel.DataAnnotations;

namespace MatchesService.Models.DTOs;

public class FoulRequest
{
    [Required]
    public string Team { get; set; } = string.Empty; // "home" | "away"

    [Range(-5, 5)]
    public int Amount { get; set; } = 1;
}
