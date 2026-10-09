using BattleGame.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Api.Data;

public class BattleGameDbContext(DbContextOptions<BattleGameDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<PlayerAsset> PlayerAssets => Set<PlayerAsset>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Player>(e =>
        {
            e.ToTable("Player");
            e.HasKey(x => x.PlayerId);
            e.Property(x => x.PlayerName).HasMaxLength(64).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(128).IsRequired();
            e.Property(x => x.Age).HasMaxLength(10).IsRequired();
            e.Property(x => x.Email).HasMaxLength(64).IsRequired();
            e.HasIndex(x => x.PlayerName).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
        });

        b.Entity<Asset>(e =>
        {
            e.ToTable("Asset");
            e.HasKey(x => x.AssetId);
            e.Property(x => x.AssetName).HasMaxLength(64).IsRequired();
        });

        b.Entity<PlayerAsset>(e =>
        {
            e.ToTable("PlayerAsset");
            e.HasKey(x => new { x.PlayerId, x.AssetId });
            e.HasOne(x => x.Player).WithMany(p => p.PlayerAssets).HasForeignKey(x => x.PlayerId);
            e.HasOne(x => x.Asset).WithMany(a => a.PlayerAssets).HasForeignKey(x => x.AssetId);
        });

        // Sample data so the getassetsbyplayer report is not empty.
        var p1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var p2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var p3 = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var a1 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var a2 = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        b.Entity<Player>().HasData(
            new Player { PlayerId = p1, PlayerName = "Player 1", FullName = "Nguyen Van A", Age = "20", Level = 10, Email = "p1@game.com" },
            new Player { PlayerId = p2, PlayerName = "Player 2", FullName = "Tran Thi B", Age = "19", Level = 3, Email = "p2@game.com" },
            new Player { PlayerId = p3, PlayerName = "Player 3", FullName = "Le Van C", Age = "23", Level = 10, Email = "p3@game.com" });
        b.Entity<Asset>().HasData(
            new Asset { AssetId = a1, AssetName = "Hero 1", LevelRequire = 1 },
            new Asset { AssetId = a2, AssetName = "Hero 2", LevelRequire = 3 });
        b.Entity<PlayerAsset>().HasData(
            new { PlayerId = p1, AssetId = a1 },
            new { PlayerId = p2, AssetId = a2 },
            new { PlayerId = p3, AssetId = a1 });
    }
}
