using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cheer.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=dpg-d8ro3ie7r5hc73ej03d0-a.oregon-postgres.render.com;Database=cheer_br_ranking;Username=cheer_br_ranking_user;Password=qF6z4mEtPakJSPhCKWkpqPyeqYffWIq5;SSL Mode=Require;Trust Server Certificate=true");

        return new AppDbContext(optionsBuilder.Options);
    }
}