using Microsoft.EntityFrameworkCore;
using MatchesService.Models;

namespace MatchesService.Data
{
    public class MatchesDbContext : DbContext
    {
        public MatchesDbContext(DbContextOptions<MatchesDbContext> options)
            : base(options) { }

        public DbSet<Match> Matches { get; set; } = null!;
        public DbSet<ScoreEvent> ScoreEvents { get; set; } = null!;
        public DbSet<Foul> Fouls { get; set; } = null!;
        public DbSet<TeamWin> TeamWins { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== MATCH =====
            modelBuilder.Entity<Match>(e =>
            {
                e.Property(p => p.Status).HasMaxLength(32);
                e.Property(p => p.QuarterDurationSeconds).HasDefaultValue(600);

                // Índices útiles
                e.HasIndex(p => p.DateMatch);
                e.HasIndex(p => new { p.Status, p.DateMatch });

                // Checks de dominio
                e.ToTable(tb =>
                {
                    tb.HasCheckConstraint("CK_Match_HomeAway_Distinct", "[HomeTeamId] <> [AwayTeamId]");
                    tb.HasCheckConstraint("CK_Match_QuarterDuration_Positive", "[QuarterDurationSeconds] > 0");
                    tb.HasCheckConstraint("CK_Match_Period_Positive", "[Period] > 0");
                });
            });

            // ===== SCORE EVENT =====
            modelBuilder.Entity<ScoreEvent>(e =>
            {
                e.Property(p => p.DateRegister).HasDefaultValueSql("GETUTCDATE()");
                e.Property(p => p.Note).HasMaxLength(200);

                e.HasIndex(p => p.MatchId);
                e.HasIndex(p => new { p.MatchId, p.TeamId });

                e.ToTable(tb =>
                {
                    tb.HasCheckConstraint("CK_ScoreEvent_Points_Range", "[Points] BETWEEN -3 AND 3");
                });

                e.HasOne(se => se.Match)
                 .WithMany(m => m.ScoreEvents)
                 .HasForeignKey(se => se.MatchId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== FOUL =====
            modelBuilder.Entity<Foul>(e =>
            {
                e.Property(p => p.DateRegister).HasDefaultValueSql("GETUTCDATE()");
                e.Property(p => p.Type).HasMaxLength(50);

                e.HasIndex(p => p.MatchId);
                e.HasIndex(p => new { p.MatchId, p.TeamId });

                e.HasOne(f => f.Match)
                 .WithMany(m => m.Fouls)
                 .HasForeignKey(f => f.MatchId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== TEAM WIN =====
            modelBuilder.Entity<TeamWin>(e =>
            {
                e.Property(p => p.DateRegistered).HasDefaultValueSql("GETUTCDATE()");
                e.HasIndex(p => p.MatchId).IsUnique(); // un ganador por partido

                e.HasOne(tw => tw.Match)
                 .WithMany()
                 .HasForeignKey(tw => tw.MatchId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
