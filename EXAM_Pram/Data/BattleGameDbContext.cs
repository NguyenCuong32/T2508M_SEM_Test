using Microsoft.EntityFrameworkCore;
using EXAM_Pram.Models;

namespace EXAM_Pram.Data
{
    public class BattleGameDbContext : DbContext
    {
        public BattleGameDbContext(DbContextOptions<BattleGameDbContext> options)
            : base(options)
        {
        }

        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<Asset> Assets { get; set; } = null!;
        public DbSet<PlayerAsset> PlayerAssets { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure PlayerAsset composite primary key
            modelBuilder.Entity<PlayerAsset>()
                .HasKey(pa => new { pa.PlayerId, pa.AssetId });

            modelBuilder.Entity<PlayerAsset>()
                .HasOne(pa => pa.Player)
                .WithMany(p => p.PlayerAssets)
                .HasForeignKey(pa => pa.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlayerAsset>()
                .HasOne(pa => pa.Asset)
                .WithMany(a => a.PlayerAssets)
                .HasForeignKey(pa => pa.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
