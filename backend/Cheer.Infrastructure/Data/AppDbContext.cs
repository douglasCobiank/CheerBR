using Cheer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cheer.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Team> Teams { get; set; }
        public DbSet<CompetitionResult> CompetitionResults { get; set; }
        public DbSet<Championship> Championships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Query filter global para soft-delete: todas as queries ignoram
            // entidades com IsDeleted=true, exceto quando explicitamente incluidas
            // via IgnoreQueryFilters().
            modelBuilder.Entity<Team>().HasQueryFilter(t => !t.IsDeleted);

            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired();
                entity.Property(e => e.Cidade).IsRequired();
                entity.Property(e => e.Categoria).IsRequired();
                entity.Property(e => e.Status).IsRequired();

                entity.HasMany(e => e.Results)
                      .WithOne(e => e.Team)
                      .HasForeignKey(e => e.TeamId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indexes para filtros comuns (listagem, dashboard, busca)
                entity.HasIndex(e => e.Categoria);
                entity.HasIndex(e => e.Cidade);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Nivel);
                entity.HasIndex(e => e.Score);
            });

            modelBuilder.Entity<CompetitionResult>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NomeCampeonato).IsRequired();
                entity.Property(e => e.Importancia).IsRequired();
                entity.Property(e => e.TipoCategoria).IsRequired();

                entity.HasIndex(e => e.Ano);

                // FK opcional para Championships (restauracao da integridade
                // referencial — antes era string livre sem FK)
                entity.HasOne(e => e.Championship)
                      .WithMany()
                      .HasForeignKey(e => e.ChampionshipId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Championship>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired();
            });
        }
    }
}
