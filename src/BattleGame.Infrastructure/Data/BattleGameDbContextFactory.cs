using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BattleGame.Infrastructure.Data;

public class BattleGameDbContextFactory : IDesignTimeDbContextFactory<BattleGameDbContext>
{
    public BattleGameDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BattleGameDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=BATTLEGAME;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new BattleGameDbContext(optionsBuilder.Options);
    }
}
