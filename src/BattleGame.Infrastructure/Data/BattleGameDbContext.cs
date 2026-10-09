using BattleGame.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Infrastructure.Data;

public class BattleGameDbContext : DbContext
{
    public BattleGameDbContext(DbContextOptions<BattleGameDbContext> options) : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<PlayerAsset> PlayerAssets => Set<PlayerAsset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Player configuration
        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("Player");
            entity.HasKey(e => e.PlayerId);
            entity.Property(e => e.PlayerName).HasMaxLength(64).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(128);
            entity.Property(e => e.Age).HasMaxLength(10);
            entity.Property(e => e.Level).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(64);
        });

        // Asset configuration
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.ToTable("Asset");
            entity.HasKey(e => e.AssetId);
            entity.Property(e => e.AssetName).HasMaxLength(64).IsRequired();
            entity.Property(e => e.LevelRequire).IsRequired();
        });

        // PlayerAsset join table configuration
        modelBuilder.Entity<PlayerAsset>(entity =>
        {
            entity.ToTable("PlayerAsset");
            entity.HasKey(pa => new { pa.PlayerId, pa.AssetId });

            entity.HasOne(pa => pa.Player)
                .WithMany(p => p.PlayerAssets)
                .HasForeignKey(pa => pa.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pa => pa.Asset)
                .WithMany(a => a.PlayerAssets)
                .HasForeignKey(pa => pa.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
