using System.ComponentModel.DataAnnotations;

namespace MatchesService.Models.DTOs;

/// <summary>
/// Ajusta el marcador (suma o resta puntos) de un equipo.
/// Usamos record class (no posicional) para poder validar por propiedades.
/// </summary>
public record class AdjustScoreDto
{
    [Required]
    public int TeamId { get; init; }

    // Puedes ajustar el rango si quieres
    [Range(-1000, 1000, ErrorMessage = "Delta fuera de rango")]
    public int Delta { get; init; }
}
