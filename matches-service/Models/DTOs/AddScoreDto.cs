using System.ComponentModel.DataAnnotations;

namespace MatchesService.Models.DTOs;

/// <summary>
/// DTO used to add a new score event to a match.
/// En .NET 8, los atributos de validación para records posicionales
/// deben ir en los parámetros del constructor.
/// </summary>
public record AddScoreDto(
    [Required] int TeamId,         // Team that scored
    int? PlayerId,                  // Player who scored (optional)
    [Range(-3, 3, ErrorMessage = "Points debe estar entre -3 y 3")] int Points  // 1, 2, 3 o negativos para corrección
);
