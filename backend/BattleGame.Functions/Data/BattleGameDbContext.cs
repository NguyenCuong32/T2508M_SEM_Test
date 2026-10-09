using System;
using BattleGame.Functions.Models;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Functions.Data
{
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

            // Configure Player
            modelBuilder.Entity<Player>(entity =>
            {
                entity.ToTable("Player");
                entity.HasKey(e => e.PlayerId);
                entity.Property(e => e.PlayerId).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.PlayerName).IsRequired().HasMaxLength(64);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(128);
                entity.Property(e => e.Age).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Level).HasColumnName("Level").HasDefaultValue(1);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(64);
            });

            // Configure Asset
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.ToTable("Asset");
                entity.HasKey(e => e.AssetId);
                entity.Property(e => e.AssetId).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.AssetName).IsRequired().HasMaxLength(64);
                entity.Property(e => e.LevelRequire).HasDefaultValue(1);
            });

            // Configure PlayerAsset composite primary key and relationships
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
}
