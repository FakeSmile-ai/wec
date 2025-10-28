using System.ComponentModel.DataAnnotations;

namespace MatchesService.Models
{
    /// <summary>Registra el equipo ganador de un partido.</summary>
    public class TeamWin
    {
        public long Id { get; set; }

        [Range(1, int.MaxValue)]
        public int TeamId { get; set; }

        public int MatchId { get; set; }
        public Match Match { get; set; } = null!;

        public DateTime DateRegistered { get; set; } = DateTime.UtcNow;
    }
}
