using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KiwiMind.Infrastructure.Persistence;

public class KiwiMindDbContextFactory : IDesignTimeDbContextFactory<KiwiMindDbContext>
{
    public KiwiMindDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<KiwiMindDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=kiwimind;Username=postgres;Password=postgres",
            npgsql => npgsql.UseVector());

        return new KiwiMindDbContext(optionsBuilder.Options);
    }
}
