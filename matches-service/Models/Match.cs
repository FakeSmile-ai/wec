using System.ComponentModel.DataAnnotations;

namespace MatchesService.Models
{
    public class Match
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        public int HomeTeamId { get; set; }

        [Range(1, int.MaxValue)]
        public int AwayTeamId { get; set; }

        [Range(0, int.MaxValue)]
        public int HomeScore { get; set; }

        [Range(0, int.MaxValue)]
        public int AwayScore { get; set; }

        [Range(1, 20)]
        public int Period { get; set; } = 1;

        [Required, MaxLength(32)]
        public string Status { get; set; } = "Scheduled";

        public DateTime DateMatch { get; set; }

        [Range(60, 7200)]
        public int QuarterDurationSeconds { get; set; } = 600;

        public ICollection<ScoreEvent> ScoreEvents { get; set; } = new List<ScoreEvent>();
        public ICollection<Foul> Fouls { get; set; } = new List<Foul>();

        // Concurrencia optimista (opcional recomendado)
        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
