// ============================================================
// AssetFlow.Infrastructure / Data / AppDbContext.cs
// Contexte Entity Framework Core - connexion SQL Server
// ============================================================

using AssetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        // Constructeur : reçoit les options depuis Program.cs
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Table Users dans la base de données
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de la table User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
                entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Role).IsRequired().HasMaxLength(50);
                entity.HasIndex(u => u.Email).IsUnique(); // Email unique
            });
        }
    }
}