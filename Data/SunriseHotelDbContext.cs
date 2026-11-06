using Microsoft.EntityFrameworkCore;
using SUNRISEHOTEL.Models;

namespace SUNRISEHOTEL.Data
{
    public class SunriseHotelDbContext : DbContext
    {
        public SunriseHotelDbContext() : base()
        {

        }
        public SunriseHotelDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Room>()
                .Property(x => x.PricePerNight)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Booking>()
                .Property(x => x.TotalPrice)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Booking>()
                .HasOne(x => x.Room)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.RoomId);
        }

        public DbSet<Room> Rooms {  get; set; }

        public DbSet<Booking> Bookings {  get; set; }
    }
}
