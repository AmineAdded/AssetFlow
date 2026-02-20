// ============================================================
// AssetFlow.Infrastructure / Data / AppDbContext.cs
// Contexte Entity Framework Core - VERSION MISE À JOUR
// Ajout des tables Materiels et Affectations
// ============================================================

using AssetFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // === TABLES ===
        public DbSet<User> Users { get; set; }
        public DbSet<Materiel> Materiels { get; set; }
        public DbSet<Affectation> Affectations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === CONFIGURATION USER ===
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
                entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Role).IsRequired().HasMaxLength(50);
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // === CONFIGURATION MATERIEL ===
            modelBuilder.Entity<Materiel>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Reference).IsRequired().HasMaxLength(100);
                entity.Property(m => m.Designation).IsRequired().HasMaxLength(200);
                entity.Property(m => m.Categorie).IsRequired().HasMaxLength(100);
                entity.Property(m => m.Unite).HasMaxLength(50);
                entity.Property(m => m.Etat).HasConversion<string>().HasMaxLength(50);
                entity.HasIndex(m => m.Reference).IsUnique();
            });

            // === CONFIGURATION AFFECTATION ===
            modelBuilder.Entity<Affectation>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Statut).HasConversion<string>().HasMaxLength(50);

                // Relation Affectation -> Materiel (1-N)
                entity.HasOne(a => a.Materiel)
                      .WithMany(m => m.Affectations)
                      .HasForeignKey(a => a.MaterielId)
                      .OnDelete(DeleteBehavior.Restrict); // Empêche suppression matériel si affectation existe

                // Relation Affectation -> User (1-N)
                entity.HasOne(a => a.Utilisateur)
                      .WithMany()
                      .HasForeignKey(a => a.UtilisateurId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}