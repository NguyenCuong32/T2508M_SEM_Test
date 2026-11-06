using Microsoft.EntityFrameworkCore;
using SunriseHotel.Models;

namespace SunriseHotel.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(SunriseDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (!await context.Room.AnyAsync())
        {
            var rooms = new List<Room>
            {
                new Room { RoomId = 1, RoomNumber = "101", RoomType = "Single", PricePerNight = 600000m, MaxGuests = 1 },
                new Room { RoomId = 2, RoomNumber = "102", RoomType = "Single", PricePerNight = 600000m, MaxGuests = 1 },
                new Room { RoomId = 3, RoomNumber = "201", RoomType = "Double", PricePerNight = 950000m, MaxGuests = 2 },
                new Room { RoomId = 4, RoomNumber = "202", RoomType = "Double", PricePerNight = 950000m, MaxGuests = 2 },
                new Room { RoomId = 5, RoomNumber = "301", RoomType = "Suite", PricePerNight = 2200000m, MaxGuests = 4 }
            };

            await context.Room.AddRangeAsync(rooms);
            await context.SaveChangesAsync();
        }

        if (!await context.Booking.AnyAsync())
        {
            var bookings = new List<Booking>
            {
                new Booking
                {
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
                    RoomId = 5,
                    GuestName = "Lê Hoàng Nam",
                    Phone = "0987654321",
                    CheckIn = new DateOnly(2026, 11, 25),
                    CheckOut = new DateOnly(2026, 11, 26),
                    TotalPrice = 2200000m,
                    Status = "Confirmed"
                }
            };

            await context.Booking.AddRangeAsync(bookings);
            await context.SaveChangesAsync();
        }
    }
}

