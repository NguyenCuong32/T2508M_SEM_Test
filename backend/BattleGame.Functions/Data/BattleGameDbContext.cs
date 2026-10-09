using BattleGame.Functions.Models;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Functions.Data;

public sealed class BattleGameDbContext(DbContextOptions<BattleGameDbContext> options)
    : DbContext(options)
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
            entity.Property(player => player.PlayerName).HasMaxLength(64).IsRequired();
            entity.Property(player => player.FullName).HasMaxLength(128).IsRequired();
            entity.Property(player => player.Email).HasMaxLength(64).IsRequired();
            entity.Property(player => player.Level).HasDefaultValue(1);
            entity.HasIndex(player => player.PlayerName).IsUnique();
            entity.HasIndex(player => player.Email).IsUnique();
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.ToTable("Asset");
            entity.HasKey(asset => asset.AssetId);
            entity.Property(asset => asset.AssetName).HasMaxLength(64).IsRequired();
            entity.HasIndex(asset => asset.AssetName).IsUnique();
        });

        modelBuilder.Entity<PlayerAsset>(entity =>
        {
            entity.ToTable("PlayerAsset");
            entity.HasKey(playerAsset => new { playerAsset.PlayerId, playerAsset.AssetId });

            entity.HasOne(playerAsset => playerAsset.Player)
                .WithMany(player => player.PlayerAssets)
                .HasForeignKey(playerAsset => playerAsset.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(playerAsset => playerAsset.Asset)
                .WithMany(asset => asset.PlayerAssets)
                .HasForeignKey(playerAsset => playerAsset.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
