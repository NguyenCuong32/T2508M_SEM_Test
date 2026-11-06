using Microsoft.EntityFrameworkCore;
using SunriseHotel.Data;

namespace SunriseHotel.Services;

public class BookingToolService : IBookingToolService
{
    private readonly SunriseHotelDbContext _dbContext;
    private readonly ILogger<BookingToolService> _logger;

    public BookingToolService(SunriseHotelDbContext dbContext, ILogger<BookingToolService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<object> CheckAvailabilityAsync(string roomType, string checkIn, string checkOut, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Tool CheckAvailability called with roomType={RoomType}, checkIn={CheckIn}, checkOut={CheckOut}", roomType, checkIn, checkOut);

        if (!DateOnly.TryParse(checkIn, out var parsedCheckIn) || !DateOnly.TryParse(checkOut, out var parsedCheckOut))
        {
            return new { success = false, error = "Invalid date format for checkIn or checkOut. Please use YYYY-MM-DD." };
        }

        if (parsedCheckOut <= parsedCheckIn)
        {
            return new { success = false, error = "CheckOut date must be strictly after CheckIn date." };
        }

        var validRoomTypes = new[] { "Single", "Double", "Suite" };
        var normalizedType = validRoomTypes.FirstOrDefault(t => t.Equals(roomType, StringComparison.OrdinalIgnoreCase));
        if (normalizedType == null)
        {
            return new { success = false, error = $"Invalid roomType '{roomType}'. Supported types: Single, Double, Suite." };
        }

        var candidateRooms = await _dbContext.Rooms
            .Where(r => r.RoomType == normalizedType)
            .ToListAsync(cancellationToken);

        if (!candidateRooms.Any())
        {
            return new { success = false, error = $"No rooms of type '{normalizedType}' found." };
        }

        var availableRooms = new List<object>();
        foreach (var room in candidateRooms)
        {
            var isBooked = await _dbContext.Bookings.AnyAsync(b =>
                b.RoomId == room.RoomId &&
                b.Status == "Confirmed" &&
                b.CheckIn < parsedCheckOut &&
                b.CheckOut > parsedCheckIn,
                cancellationToken);

            if (!isBooked)
            {
                availableRooms.Add(new
                {
                    roomId = room.RoomId,
                    roomNumber = room.RoomNumber,
                    roomType = room.RoomType,
                    pricePerNight = room.PricePerNight,
                    maxGuests = room.MaxGuests
                });
            }
        }

        var nights = parsedCheckOut.DayNumber - parsedCheckIn.DayNumber;
        return new
        {
            success = true,
            roomType = normalizedType,
            checkIn = parsedCheckIn.ToString("yyyy-MM-dd"),
            checkOut = parsedCheckOut.ToString("yyyy-MM-dd"),
            nights,
            availableRoomsCount = availableRooms.Count,
            availableRooms
        };
    }

    public async Task<object> GetBookingsByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Tool GetBookingsByPhone called with phone={Phone}", phone);

        if (string.IsNullOrWhiteSpace(phone))
        {
            return new { success = false, error = "Phone number is required." };
        }

        var cleanPhone = phone.Trim();
        var bookings = await _dbContext.Bookings
            .Include(b => b.Room)
            .Where(b => b.Phone == cleanPhone)
            .OrderByDescending(b => b.BookingId)
            .Select(b => new
            {
                bookingId = b.BookingId,
                roomId = b.RoomId,
                roomNumber = b.Room != null ? b.Room.RoomNumber : "N/A",
                roomType = b.Room != null ? b.Room.RoomType : "N/A",
                guestName = b.GuestName,
                phone = b.Phone,
                checkIn = b.CheckIn.ToString("yyyy-MM-dd"),
                checkOut = b.CheckOut.ToString("yyyy-MM-dd"),
                totalPrice = b.TotalPrice,
                status = b.Status
            })
            .ToListAsync(cancellationToken);

        return new
        {
            success = true,
            phone = cleanPhone,
            count = bookings.Count,
            bookings
        };
    }

    public async Task<object> CancelBookingAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Tool CancelBooking called with bookingId={BookingId}", bookingId);

        var booking = await _dbContext.Bookings
            .Include(b => b.Room)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId, cancellationToken);

        if (booking == null)
        {
            return new { success = false, error = $"Booking with ID {bookingId} not found." };
        }

        if (booking.Status == "Cancelled")
        {
            return new { success = false, message = $"Booking {bookingId} has already been cancelled." };
        }

        booking.Status = "Cancelled";
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new
        {
            success = true,
            bookingId = booking.BookingId,
            guestName = booking.GuestName,
            status = booking.Status,
            message = $"Booking {bookingId} has been successfully cancelled."
        };
    }
}
