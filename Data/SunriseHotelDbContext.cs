using Microsoft.EntityFrameworkCore;
using SunriseHotel.Models;

namespace SunriseHotel.Data;

public class SunriseHotelDbContext : DbContext
{
    public SunriseHotelDbContext(DbContextOptions<SunriseHotelDbContext> options)
        : base(options)
    {
    }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Room table configuration
        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("Room");
            entity.HasKey(r => r.RoomId);
            entity.Property(r => r.RoomNumber).HasMaxLength(10).IsRequired();
            entity.Property(r => r.RoomType).HasMaxLength(32).IsRequired();
            entity.Property(r => r.PricePerNight).HasPrecision(18, 2);
            entity.Property(r => r.MaxGuests).IsRequired();

            // Seed initial data according to exam requirements
            entity.HasData(
                new Room { RoomId = 1, RoomNumber = "101", RoomType = "Single", PricePerNight = 600000m, MaxGuests = 1 },
                new Room { RoomId = 2, RoomNumber = "102", RoomType = "Single", PricePerNight = 600000m, MaxGuests = 1 },
                new Room { RoomId = 3, RoomNumber = "201", RoomType = "Double", PricePerNight = 950000m, MaxGuests = 2 },
                new Room { RoomId = 4, RoomNumber = "202", RoomType = "Double", PricePerNight = 950000m, MaxGuests = 2 },
                new Room { RoomId = 5, RoomNumber = "301", RoomType = "Suite", PricePerNight = 2200000m, MaxGuests = 4 }
            );
        });

        // Booking table configuration
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("Booking");
            entity.HasKey(b => b.BookingId);
            entity.Property(b => b.BookingId).ValueGeneratedOnAdd();
            entity.Property(b => b.GuestName).HasMaxLength(128).IsRequired();
            entity.Property(b => b.Phone).HasMaxLength(20).IsRequired();
            entity.Property(b => b.TotalPrice).HasPrecision(18, 2);
            entity.Property(b => b.Status).HasMaxLength(16).IsRequired();

            entity.HasOne(b => b.Room)
                  .WithMany(r => r.Bookings)
                  .HasForeignKey(b => b.RoomId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
