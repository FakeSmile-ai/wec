// DTOs/TeamDto.cs
namespace MatchesService.Models.DTOs
{
    public class TeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Color { get; set; }
    }
}
