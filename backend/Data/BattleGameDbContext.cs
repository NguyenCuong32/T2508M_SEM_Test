using BattleGame.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Api.Data;

public class BattleGameDbContext : DbContext
{
    public BattleGameDbContext(DbContextOptions<BattleGameDbContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<PlayerAsset> PlayerAssets => Set<PlayerAsset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Composite Key for PlayerAsset
        modelBuilder.Entity<PlayerAsset>()
            .HasKey(pa => new { pa.PlayerId, pa.AssetId });

        // Configure Player - PlayerAsset relationship
        modelBuilder.Entity<PlayerAsset>()
            .HasOne(pa => pa.Player)
            .WithMany(p => p.PlayerAssets)
            .HasForeignKey(pa => pa.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Asset - PlayerAsset relationship
        modelBuilder.Entity<PlayerAsset>()
            .HasOne(pa => pa.Asset)
            .WithMany(a => a.PlayerAssets)
            .HasForeignKey(pa => pa.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
