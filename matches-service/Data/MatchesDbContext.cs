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
                e.Property(p => p.TimeRemaining).HasDefaultValue(600);
                e.Property(p => p.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");

                e.HasIndex(p => p.DateTime);
                e.HasIndex(p => new { p.Status, p.DateTime });

                e.ToTable(tb =>
                {
                    tb.HasCheckConstraint("CK_Match_HomeAway_Distinct", "[HomeTeamId] <> [AwayTeamId]");
                    tb.HasCheckConstraint("CK_Match_Quarter_Positive", "[Quarter] >= 1");
                });
            });
        }
    }
}
