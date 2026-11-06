using Microsoft.EntityFrameworkCore;
using SunriseHotel.Models;

namespace SunriseHotel.Data
{
    public class HotelDBContext : DbContext
    {
        public HotelDBContext(DbContextOptions<HotelDBContext> options) : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Room> Rooms { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Room Configuration
            modelBuilder.Entity<Room>(entity =>
            {
                entity.ToTable("Room");
                entity.HasKey(r => r.roomId);
                entity.Property(r => r.PricePerNight)
                      .HasColumnType("decimal(18,2)");
                entity.HasIndex(r => r.roomNumber)
                      .IsUnique();
            });

            // Booking Configuration
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("Booking");
                entity.HasKey(b => b.BookingId);
                entity.Property(b => b.TotalPrice)
                      .HasColumnType("decimal(18,2)");
                entity.Property(b => b.CheckIn)
                      .HasColumnType("date");
                entity.Property(b => b.CheckOut)
                      .HasColumnType("date");
                entity.Property(b => b.Status)
                      .HasDefaultValue("Confirmed");
                entity.HasOne(b => b.Room)
                      .WithMany(r => r.Bookings)
                      .HasForeignKey(b => b.RoomId);
            }); 
        }
    }
}
