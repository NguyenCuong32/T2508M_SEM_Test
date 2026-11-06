using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SunriseHotel.Domain.Entities;
using SunriseHotel.Domain.Enums;

namespace SunriseHotel.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Room", t =>
        {
            t.HasCheckConstraint("CK_Room_RoomType", "[RoomType] IN ('Single','Double','Suite')");
            t.HasCheckConstraint("CK_Room_MaxGuests", "[MaxGuests] > 0");
            t.HasCheckConstraint("CK_Room_PricePerNight", "[PricePerNight] >= 0");
        });

        builder.HasKey(r => r.RoomId);
        builder.Property(r => r.RoomId).ValueGeneratedNever();
        builder.Property(r => r.RoomNumber).HasMaxLength(10).IsRequired();
        builder.HasIndex(r => r.RoomNumber).IsUnique();
        builder.Property(r => r.RoomType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(r => r.PricePerNight).HasColumnType("decimal(18,2)");
        builder.Property(r => r.MaxGuests);

        builder.HasData(
            new Room { RoomId = 1, RoomNumber = "101", RoomType = RoomType.Single, PricePerNight = 600_000m, MaxGuests = 1 },
            new Room { RoomId = 2, RoomNumber = "102", RoomType = RoomType.Single, PricePerNight = 600_000m, MaxGuests = 1 },
            new Room { RoomId = 3, RoomNumber = "201", RoomType = RoomType.Double, PricePerNight = 950_000m, MaxGuests = 2 },
            new Room { RoomId = 4, RoomNumber = "202", RoomType = RoomType.Double, PricePerNight = 950_000m, MaxGuests = 2 },
            new Room { RoomId = 5, RoomNumber = "301", RoomType = RoomType.Suite, PricePerNight = 2_200_000m, MaxGuests = 4 });
    }
}
