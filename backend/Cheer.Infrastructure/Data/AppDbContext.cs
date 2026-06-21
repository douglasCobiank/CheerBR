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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
            });

            modelBuilder.Entity<CompetitionResult>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NomeCampeonato).IsRequired();
                entity.Property(e => e.Importancia).IsRequired();
                entity.Property(e => e.TipoCategoria).IsRequired();
            });
        }
    }
}
