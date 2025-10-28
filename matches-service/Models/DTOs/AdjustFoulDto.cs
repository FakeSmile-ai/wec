using System.ComponentModel.DataAnnotations;

namespace MatchesService.Models.DTOs;

/// <summary>
/// Ajusta la cantidad de faltas de un equipo.
/// </summary>
public record class AdjustFoulDto
{
    [Required]
    public int TeamId { get; init; }

    [Range(-1000, 1000, ErrorMessage = "Delta fuera de rango")]
    public int Delta { get; init; }
}
