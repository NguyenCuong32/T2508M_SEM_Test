using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BattleGame.Functions.Data;

public sealed class BattleGameDbContextFactory : IDesignTimeDbContextFactory<BattleGameDbContext>
{
    public BattleGameDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("BATTLEGAME_CONNECTION_STRING")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=BATTLEGAME;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<BattleGameDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new BattleGameDbContext(options);
    }
}
