using Microsoft.EntityFrameworkCore;
using baithi.Models;

namespace baithi.Repositories;

public class SunriseDbContext : DbContext
{
    public SunriseDbContext(DbContextOptions<SunriseDbContext> options) : base(options)
    {
    }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("Room");
            entity.HasKey(e => e.RoomId);
            entity.Property(e => e.RoomId).ValueGeneratedNever();
            entity.Property(e => e.RoomNumber).HasMaxLength(10).IsRequired();
            entity.Property(e => e.RoomType).HasMaxLength(32).IsRequired();
            entity.Property(e => e.PricePerNight).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MaxGuests).IsRequired();

            entity.HasData(
                new Room { RoomId = 1, RoomNumber = "101", RoomType = "Single", PricePerNight = 600000m, MaxGuests = 1 },
                new Room { RoomId = 2, RoomNumber = "102", RoomType = "Single", PricePerNight = 600000m, MaxGuests = 1 },
                new Room { RoomId = 3, RoomNumber = "201", RoomType = "Double", PricePerNight = 950000m, MaxGuests = 2 },
                new Room { RoomId = 4, RoomNumber = "202", RoomType = "Double", PricePerNight = 950000m, MaxGuests = 2 },
                new Room { RoomId = 5, RoomNumber = "301", RoomType = "Suite", PricePerNight = 2200000m, MaxGuests = 4 }
            );
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("Booking");
            entity.HasKey(e => e.BookingId);
            entity.Property(e => e.BookingId).ValueGeneratedOnAdd();
            entity.Property(e => e.GuestName).HasMaxLength(128).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20).IsRequired();
            entity.Property(e => e.CheckIn).HasColumnType("date").IsRequired();
            entity.Property(e => e.CheckOut).HasColumnType("date").IsRequired();
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Status).HasMaxLength(16).IsRequired();

            entity.HasOne(e => e.Room)
                  .WithMany(r => r.Bookings)
                  .HasForeignKey(e => e.RoomId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasData(
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
                    Phone = "0902345678",
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
                    Phone = "0903456789",
                    CheckIn = new DateOnly(2026, 11, 25),
                    CheckOut = new DateOnly(2026, 11, 26),
                    TotalPrice = 2200000m,
                    Status = "Confirmed"
                }
            );
        });
    }
}

