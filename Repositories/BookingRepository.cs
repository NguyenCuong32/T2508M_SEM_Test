using Microsoft.EntityFrameworkCore;
using baithi.Models;

namespace baithi.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly SunriseDbContext _context;
    private readonly ILogger<BookingRepository> _logger;

    public BookingRepository(SunriseDbContext context, ILogger<BookingRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task EnsureDatabaseCreatedAndSeededAsync()
    {
        try
        {
            await _context.Database.EnsureCreatedAsync();

            if (!await _context.Rooms.AnyAsync())
            {
                var initialRooms = new List<Room>
                {
                    new() { RoomId = 1, RoomNumber = "101", RoomType = "Single", PricePerNight = 600000m, MaxGuests = 1 },
                    new() { RoomId = 2, RoomNumber = "102", RoomType = "Single", PricePerNight = 600000m, MaxGuests = 1 },
                    new() { RoomId = 3, RoomNumber = "201", RoomType = "Double", PricePerNight = 950000m, MaxGuests = 2 },
                    new() { RoomId = 4, RoomNumber = "202", RoomType = "Double", PricePerNight = 950000m, MaxGuests = 2 },
                    new() { RoomId = 5, RoomNumber = "301", RoomType = "Suite", PricePerNight = 2200000m, MaxGuests = 4 }
                };
                await _context.Rooms.AddRangeAsync(initialRooms);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Seeded 5 rooms into database.");
            }

            if (!await _context.Bookings.AnyAsync())
            {
                var initialBookings = new List<Booking>
                {
                    new()
                    {
                        RoomId = 3,
                        GuestName = "Nguyễn Văn An",
                        Phone = "0901234567",
                        CheckIn = new DateOnly(2026, 11, 20),
                        CheckOut = new DateOnly(2026, 11, 22),
                        TotalPrice = 1900000m,
                        Status = "Confirmed"
                    },
                    new()
                    {
                        RoomId = 1,
                        GuestName = "Trần Thị Bình",
                        Phone = "0902345678",
                        CheckIn = new DateOnly(2026, 11, 21),
                        CheckOut = new DateOnly(2026, 11, 24),
                        TotalPrice = 1800000m,
                        Status = "Confirmed"
                    },
                    new()
                    {
                        RoomId = 5,
                        GuestName = "Lê Hoàng Nam",
                        Phone = "0903456789",
                        CheckIn = new DateOnly(2026, 11, 25),
                        CheckOut = new DateOnly(2026, 11, 26),
                        TotalPrice = 2200000m,
                        Status = "Confirmed"
                    }
                };
                await _context.Bookings.AddRangeAsync(initialBookings);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Seeded 3 confirmed bookings into database.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while ensuring database creation and seeding.");
            throw;
        }
    }

    public async Task<List<Room>> GetAvailableRoomsAsync(string? roomType, DateOnly checkIn, DateOnly checkOut)
    {
        var bookedRoomIds = await _context.Bookings
            .Where(b => b.Status == "Confirmed" && b.CheckIn < checkOut && checkIn < b.CheckOut)
            .Select(b => b.RoomId)
            .Distinct()
            .ToListAsync();

        var query = _context.Rooms.Where(r => !bookedRoomIds.Contains(r.RoomId));

        if (!string.IsNullOrWhiteSpace(roomType))
        {
            var normalizedType = roomType.Trim().ToLower();
            query = query.Where(r => r.RoomType.ToLower() == normalizedType);
        }

        return await query.OrderBy(r => r.RoomNumber).ToListAsync();
    }

    public async Task<Room?> GetFirstAvailableRoomAsync(string roomType, DateOnly checkIn, DateOnly checkOut)
    {
        var bookedRoomIds = await _context.Bookings
            .Where(b => b.Status == "Confirmed" && b.CheckIn < checkOut && checkIn < b.CheckOut)
            .Select(b => b.RoomId)
            .Distinct()
            .ToListAsync();

        var normalizedType = roomType.Trim().ToLower();

        return await _context.Rooms
            .Where(r => !bookedRoomIds.Contains(r.RoomId) && r.RoomType.ToLower() == normalizedType)
            .OrderBy(r => r.RoomNumber)
            .FirstOrDefaultAsync();
    }

    public async Task<Room?> GetRoomByIdAsync(int roomId)
    {
        return await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId);
    }

    public async Task<Booking> CreateBookingAsync(Booking booking)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        await _context.Entry(booking).Reference(b => b.Room).LoadAsync();
        return booking;
    }

    public async Task<List<Booking>> GetBookingsByPhoneAsync(string phone)
    {
        var cleanPhone = phone.Trim();
        return await _context.Bookings
            .Include(b => b.Room)
            .Where(b => b.Phone == cleanPhone)
            .OrderByDescending(b => b.BookingId)
            .ToListAsync();
    }

    public async Task<Booking?> GetBookingByIdAsync(int bookingId)
    {
        return await _context.Bookings
            .Include(b => b.Room)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);
    }

    public async Task<Booking?> CancelBookingAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Room)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

        if (booking == null) return null;

        booking.Status = "Cancelled";
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<List<Booking>> GetConfirmedBookingsReportAsync(DateOnly? from, DateOnly? to)
    {
        var query = _context.Bookings
            .Include(b => b.Room)
            .Where(b => b.Status == "Confirmed");

        if (from.HasValue)
        {
            query = query.Where(b => b.CheckIn >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(b => b.CheckIn <= to.Value);
        }

        return await query.OrderBy(b => b.CheckIn).ThenBy(b => b.BookingId).ToListAsync();
    }
}

