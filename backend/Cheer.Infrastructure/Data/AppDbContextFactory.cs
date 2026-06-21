using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cheer.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=db.ddibhzidgedzpfwwhczt.supabase.co;Database=postgres;Username=postgres;Password=procheer2025;SSL Mode=Require;Trust Server Certificate=true");

        return new AppDbContext(optionsBuilder.Options);
    }
}