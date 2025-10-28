using System.ComponentModel.DataAnnotations;

namespace MatchesService.Models
{
    /// <summary>Evento de anotación o ajuste de puntos.</summary>
    public class ScoreEvent
    {
        public long Id { get; set; }

        public int MatchId { get; set; }
        public Match Match { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int TeamId { get; set; }

        public int? PlayerId { get; set; }

        /// <summary>Puntos (+/-) para correcciones.</summary>
        [Range(-3, 3)]
        public int Points { get; set; }

        [MaxLength(200)]
        public string? Note { get; set; }

        /// <summary>Fecha y hora de registro (UTC).</summary>
        public DateTime DateRegister { get; set; } = DateTime.UtcNow;
    }
}
