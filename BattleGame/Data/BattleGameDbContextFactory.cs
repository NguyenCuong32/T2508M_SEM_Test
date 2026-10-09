using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BattleGame.Data;

public sealed class BattleGameDbContextFactory : IDesignTimeDbContextFactory<BattleGameDbContext>
{
    public BattleGameDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("local.settings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        var options = new DbContextOptionsBuilder<BattleGameDbContext>()
            .UseSqlServer(DatabaseConfiguration.GetConnectionString(configuration)).Options;
        return new BattleGameDbContext(options);
    }
}
