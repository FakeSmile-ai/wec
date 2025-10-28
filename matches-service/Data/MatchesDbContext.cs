using Microsoft.EntityFrameworkCore;
using MatchesService.Models;

namespace MatchesService.Data
{
    public class MatchesDbContext : DbContext
    {
        public MatchesDbContext(DbContextOptions<MatchesDbContext> options)
            : base(options) { }

        public DbSet<Match> Matches { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== MATCH =====
            modelBuilder.Entity<Match>(e =>
            {
                e.Property(p => p.Status).HasMaxLength(32);
                e.Property(p => p.FoulsHome).HasDefaultValue(0);
                e.Property(p => p.FoulsAway).HasDefaultValue(0);
                e.Property(p => p.Quarter).HasDefaultValue(1);
                e.Property(p => p.TimeRemaining).HasDefaultValue(600);
                e.Property(p => p.TimerRunning).HasDefaultValue(false);
                e.Property(p => p.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");

                e.HasIndex(p => p.DateTime);
                e.HasIndex(p => new { p.Status, p.DateTime });

                e.ToTable(tb =>
                {
                    tb.HasCheckConstraint("CK_Match_HomeAway_Distinct", "[HomeTeamId] <> [AwayTeamId]");
                    tb.HasCheckConstraint("CK_Match_Quarter_Range", "[Quarter] BETWEEN 1 AND 4");
                    tb.HasCheckConstraint("CK_Match_Fouls_NonNegative", "[FoulsHome] >= 0 AND [FoulsAway] >= 0");
                    tb.HasCheckConstraint("CK_Match_TimeRemaining_NonNegative", "[TimeRemaining] >= 0");
                });
            });
        }
    }
}
