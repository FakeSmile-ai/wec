using System.ComponentModel.DataAnnotations;

namespace MatchesService.Models
{
    /// <summary>Representa una falta cometida por un equipo o jugador en un partido.</summary>
    public class Foul
    {
        public long Id { get; set; }

        public int MatchId { get; set; }
        public Match Match { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int TeamId { get; set; }

        public int? PlayerId { get; set; }

        /// <summary>Tipo de falta: Personal, Técnica, etc.</summary>
        [MaxLength(50)]
        public string? Type { get; set; }

        /// <summary>Fecha y hora de registro (UTC).</summary>
        public DateTime DateRegister { get; set; } = DateTime.UtcNow;
    }
}
