using Microsoft.EntityFrameworkCore;

namespace BattleGameApi
{
    public class AppDbContext : DbContext
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<PlayerAsset> PlayerAssets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                // Chuỗi kết nối kết nối MySQL localhost với tài khoản root, không có mật khẩu
                string connStr = "Server=localhost;Database=BATTLEGAME;User=root;Password=;";
                options.UseMySql(connStr, ServerVersion.AutoDetect(connStr));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlayerAsset>().HasKey(pa => new { pa.PlayerId, pa.AssetId });
        }
    }
}