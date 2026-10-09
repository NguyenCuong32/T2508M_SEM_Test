using Microsoft.EntityFrameworkCore;
using BattleGameFunction.Models;

namespace BattleGameFunction.Data;

public class BattleGameDbContext : DbContext
{
    public BattleGameDbContext(DbContextOptions<BattleGameDbContext> options) : base(options)
    {
    }

    public DbSet<Player> Player { get; set; } = null!;
    public DbSet<Asset> Asset { get; set; } = null!;
    public DbSet<PlayerAsset> PlayerAsset { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Player table
        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("Player");
            entity.HasKey(e => e.PlayerId);
            entity.Property(e => e.PlayerId).HasMaxLength(36);
            entity.Property(e => e.PlayerName).HasMaxLength(64).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(128).IsRequired();
            entity.Property(e => e.Age).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Level).HasColumnName("Level").IsRequired();
            entity.Property(e => e.Email).HasMaxLength(64).IsRequired();
        });

        // Asset table
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.ToTable("Asset");
            entity.HasKey(e => e.AssetId);
            entity.Property(e => e.AssetId).HasMaxLength(36);
            entity.Property(e => e.AssetName).HasMaxLength(64).IsRequired();
            entity.Property(e => e.LevelRequire).IsRequired();
        });

        // PlayerAsset table
        modelBuilder.Entity<PlayerAsset>(entity =>
        {
            entity.ToTable("PlayerAsset");
            entity.HasKey(e => new { e.PlayerId, e.AssetId });

            entity.HasOne(e => e.Player)
                  .WithMany(p => p.PlayerAssets)
                  .HasForeignKey(e => e.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Asset)
                  .WithMany(a => a.PlayerAssets)
                  .HasForeignKey(e => e.AssetId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
