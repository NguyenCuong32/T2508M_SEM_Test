using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SunriseHotel.Domain.Entities;

namespace SunriseHotel.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Booking", t =>
        {
            t.HasCheckConstraint("CK_Booking_Dates", "[CheckOut] > [CheckIn]");
            t.HasCheckConstraint("CK_Booking_Status", "[Status] IN ('Confirmed','Cancelled')");
            t.HasCheckConstraint("CK_Booking_TotalPrice", "[TotalPrice] >= 0");
        });

        builder.HasKey(b => b.BookingId);
        builder.Property(b => b.BookingId).UseIdentityColumn();
        builder.Property(b => b.GuestName).HasMaxLength(128).IsRequired();
        builder.Property(b => b.Phone).HasMaxLength(20).IsRequired();
        builder.Property(b => b.CheckIn).HasColumnType("date");
        builder.Property(b => b.CheckOut).HasColumnType("date");
        builder.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Ignore(b => b.Nights);

        builder.HasOne(b => b.Room)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.RoomId, b.CheckIn, b.CheckOut });
        builder.HasIndex(b => b.Phone);
    }
}
