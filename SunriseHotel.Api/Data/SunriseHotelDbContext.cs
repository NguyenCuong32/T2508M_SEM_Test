using Microsoft.EntityFrameworkCore;
using SunriseHotel.Api.Models;

namespace SunriseHotel.Api.Data;

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

        modelBuilder.Entity<Room>(entity =>
        {
            entity.Property(r => r.PricePerNight)
                .HasPrecision(18, 2);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(b => b.TotalPrice)
                .HasPrecision(18, 2);

            entity.HasOne(b => b.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed Initial Rooms
        modelBuilder.Entity<Room>().HasData(
            new Room { RoomId = 1, RoomNumber = "101", RoomType = "Single", PricePerNight = 600000m, MaxGuests = 1 },
            new Room { RoomId = 2, RoomNumber = "102", RoomType = "Single", PricePerNight = 600000m, MaxGuests = 1 },
            new Room { RoomId = 3, RoomNumber = "201", RoomType = "Double", PricePerNight = 950000m, MaxGuests = 2 },
            new Room { RoomId = 4, RoomNumber = "202", RoomType = "Double", PricePerNight = 950000m, MaxGuests = 2 },
            new Room { RoomId = 5, RoomNumber = "301", RoomType = "Suite", PricePerNight = 2200000m, MaxGuests = 4 }
        );

        // Seed Sample Bookings (matching Question 4 report)
        modelBuilder.Entity<Booking>().HasData(
            new Booking
            {
                BookingId = 1,
                RoomId = 3,
                GuestName = "Nguyễn Văn An",
                Phone = "0901234567",
                CheckIn = new DateOnly(2026, 11, 20),
                CheckOut = new DateOnly(2026, 11, 22),
                TotalPrice = 1900000m,
                Status = "Confirmed"
            },
            new Booking
            {
                BookingId = 2,
                RoomId = 1,
                GuestName = "Trần Thị Bình",
                Phone = "0912345678",
                CheckIn = new DateOnly(2026, 11, 21),
                CheckOut = new DateOnly(2026, 11, 24),
                TotalPrice = 1800000m,
                Status = "Confirmed"
            },
            new Booking
            {
                BookingId = 3,
                RoomId = 5,
                GuestName = "Lê Hoàng Nam",
                Phone = "0987654321",
                CheckIn = new DateOnly(2026, 11, 25),
                CheckOut = new DateOnly(2026, 11, 26),
                TotalPrice = 2200000m,
                Status = "Confirmed"
            }
        );
    }
}

