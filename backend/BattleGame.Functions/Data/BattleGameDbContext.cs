using BattleGame.Functions.Models;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Functions.Data;

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
            entity.HasKey(player => player.Id);
            entity.Property(player => player.PlayerName).HasMaxLength(50).IsRequired();
            entity.HasIndex(player => player.PlayerName).IsUnique();
            entity.Property(player => player.FullName).HasMaxLength(120).IsRequired();
            entity.Property(player => player.RegisteredAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.ToTable("Asset");
            entity.HasKey(asset => asset.Id);
            entity.Property(asset => asset.Name).HasMaxLength(100).IsRequired();
            entity.Property(asset => asset.Type).HasMaxLength(30).IsRequired();
            entity.Property(asset => asset.Description).HasMaxLength(500);
            entity.HasIndex(asset => new { asset.Name, asset.Type }).IsUnique();
        });

        modelBuilder.Entity<PlayerAsset>(entity =>
        {
            entity.ToTable("PlayerAsset");
            entity.HasKey(playerAsset => playerAsset.Id);
            entity.HasIndex(playerAsset => new { playerAsset.PlayerId, playerAsset.AssetId }).IsUnique();
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
