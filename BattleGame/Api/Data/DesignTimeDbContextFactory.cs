using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BattleGame.Api.Data;

/// <summary>Used only by `dotnet ef` so migrations can be created without starting the Functions host.</summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BattleGameDbContext>
{
    public BattleGameDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BattleGameDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=BATTLEGAME;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        return new BattleGameDbContext(options);
    }
}
