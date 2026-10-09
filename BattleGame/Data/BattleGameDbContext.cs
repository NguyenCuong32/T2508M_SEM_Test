using BattleGame.Models;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Data;

public sealed class BattleGameDbContext(DbContextOptions<BattleGameDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<PlayerAsset> PlayerAssets => Set<PlayerAsset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("Player");
            entity.HasKey(player => player.PlayerId);
            entity.Property(player => player.PlayerName).IsRequired().HasMaxLength(64);
            entity.Property(player => player.FullName).IsRequired().HasMaxLength(128);
            entity.Property(player => player.Age).IsRequired().HasMaxLength(10);
            entity.Property(player => player.Level).IsRequired();
            entity.Property(player => player.Email).IsRequired().HasMaxLength(64);
            entity.HasIndex(player => player.Email).IsUnique();
        });
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.ToTable("Asset");
            entity.HasKey(asset => asset.AssetId);
            entity.Property(asset => asset.AssetName).IsRequired().HasMaxLength(64);
            entity.Property(asset => asset.LevelRequire).IsRequired();
        });
        modelBuilder.Entity<PlayerAsset>(entity =>
        {
            entity.ToTable("PlayerAsset");
            entity.HasKey(playerAsset => new { playerAsset.PlayerId, playerAsset.AssetId });
            entity.HasOne(playerAsset => playerAsset.Player).WithMany(player => player.PlayerAssets)
                .HasForeignKey(playerAsset => playerAsset.PlayerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(playerAsset => playerAsset.Asset).WithMany(asset => asset.PlayerAssets)
                .HasForeignKey(playerAsset => playerAsset.AssetId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
