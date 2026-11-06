using Microsoft.EntityFrameworkCore;
using SunriseHotel.Api.Models;

namespace SunriseHotel.Api.Data;

public class SunriseHotelDbContext : DbContext
{
    public SunriseHotelDbContext(DbContextOptions<SunriseHotelDbContext> options) : base(options)
    {
    }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId);
            entity.Property(e => e.PricePerNight).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);

            entity.HasOne(e => e.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
