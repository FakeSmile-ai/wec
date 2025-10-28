// Ruta sugerida: Infrastructure/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using AuthService.Models.Entities;

namespace AuthService.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Menu> Menus => Set<Menu>();
        public DbSet<RoleMenu> RoleMenus => Set<RoleMenu>();

        protected override void OnModelCreating(ModelBuilder model)
        {
            // ===== User =====
            model.Entity<User>(e =>
            {
                e.ToTable("Users");
                e.HasKey(x => x.Id);

                e.Property(x => x.Username)
                    .IsRequired()
                    .HasMaxLength(64);

                e.Property(x => x.Password)
                    .IsRequired()
                    .HasMaxLength(256);

                e.HasIndex(x => x.Username)
                    .IsUnique();

                e.HasOne(x => x.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Role =====
            model.Entity<Role>(e =>
            {
                e.ToTable("Roles");
                e.HasKey(x => x.Id);

                e.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(64);

                e.HasIndex(x => x.Name)
                    .IsUnique();
            });

            // ===== Menu =====
            model.Entity<Menu>(e =>
            {
                e.ToTable("Menus");
                e.HasKey(x => x.Id);

                e.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(64);

                e.Property(x => x.Url)
                    .IsRequired()
                    .HasMaxLength(128);

                e.HasIndex(x => x.Url)
                    .IsUnique();
            });

            // ===== RoleMenu (join) =====
            model.Entity<RoleMenu>(e =>
            {
                e.ToTable("RoleMenus");
                e.HasKey(x => x.Id);

                e.HasIndex(x => new { x.RoleId, x.MenuId })
                    .IsUnique();

                e.HasOne(x => x.Role)
                    .WithMany(r => r.RoleMenus)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Menu)
                    .WithMany(m => m.RoleMenus)
                    .HasForeignKey(x => x.MenuId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Seed determinista (SIN valores dinámicos) =====
            var seedCreatedAt = new DateTime(2025, 10, 18, 0, 0, 0, DateTimeKind.Utc);

            model.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", CreatedAt = seedCreatedAt, CreatedBy = 0, UpdatedAt = null, UpdatedBy = 0 },
                new Role { Id = 2, Name = "User",  CreatedAt = seedCreatedAt, CreatedBy = 0, UpdatedAt = null, UpdatedBy = 0 }
            );
        }
    }
}
