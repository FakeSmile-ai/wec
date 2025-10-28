using System.ComponentModel.DataAnnotations;

namespace MatchesService.Models.DTOs
{
    /// <summary>DTO para iniciar el temporizador de un partido.</summary>
    public record StartTimerDto([property: Range(1, 7200)] int Seconds);
}
