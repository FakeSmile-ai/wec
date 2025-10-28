using System.ComponentModel.DataAnnotations;

namespace MatchesService.Models.DTOs
{
    public record AddFoulDto(
        [property: Range(1, int.MaxValue)] int TeamId,
        int? PlayerId,
        [property: MaxLength(50)] string? Type
    );
}
