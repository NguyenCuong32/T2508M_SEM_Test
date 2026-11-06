using Microsoft.EntityFrameworkCore;
using SunriseHotelAI.Models;

namespace SunriseHotelAI.Data;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
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
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).HasMaxLength(16).IsRequired();

            entity.HasOne(e => e.Room)
                  .WithMany(r => r.Bookings)
                  .HasForeignKey(e => e.RoomId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
