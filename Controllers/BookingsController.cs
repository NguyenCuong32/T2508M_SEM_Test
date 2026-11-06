using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunriseHotel.Data;
using SunriseHotel.DTOs;
using SunriseHotel.Models;
using SunriseHotel.Services;

namespace SunriseHotel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly SunriseHotelDbContext _dbContext;
    private readonly IGeminiService _geminiService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(
        SunriseHotelDbContext dbContext,
        IGeminiService geminiService,
        ILogger<BookingsController> logger)
    {
        _dbContext = dbContext;
        _geminiService = geminiService;
        _logger = logger;
    }

    [HttpPost("parse")]
    public async Task<IActionResult> ParseAndBook([FromBody] ParseBookingRequest request, CancellationToken cancellationToken)
    {
        // 1. Validate natural language input
        if (request == null || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new { error = "Booking text is required." });
        }

        // 2 & 3. Send text to Gemini to extract JSON
        ParsedBookingData? parsedData;
        try
        {
            parsedData = await _geminiService.ParseBookingAsync(request.Text, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call Gemini for parsing booking.");
            return StatusCode(502, new { error = "AI service unavailable. Please try again later." });
        }

        // 4 & 5. Validate extracted JSON data
        if (parsedData == null)
        {
            return BadRequest(new { error = "Could not extract booking details from the provided text." });
        }

        if (string.IsNullOrWhiteSpace(parsedData.GuestName))
        {
            return BadRequest(new { error = "Guest name is missing in booking details." });
        }

        if (string.IsNullOrWhiteSpace(parsedData.Phone))
        {
            return BadRequest(new { error = "Phone number is missing in booking details." });
        }

        if (!DateOnly.TryParse(parsedData.CheckIn, out var checkIn) ||
            !DateOnly.TryParse(parsedData.CheckOut, out var checkOut))
        {
            return BadRequest(new { error = "Invalid CheckIn or CheckOut date format. Expected YYYY-MM-DD." });
        }

        // 6. Validate CheckOut > CheckIn
        if (checkOut <= checkIn)
        {
            return BadRequest(new { error = "CheckOut date must be strictly after CheckIn date." });
        }

        // 7. Validate roomType: Single, Double, Suite
        var validRoomTypes = new[] { "Single", "Double", "Suite" };
        var normalizedRoomType = validRoomTypes.FirstOrDefault(t => t.Equals(parsedData.RoomType, StringComparison.OrdinalIgnoreCase));
        if (normalizedRoomType == null)
        {
            return BadRequest(new { error = $"Invalid RoomType '{parsedData.RoomType}'. Supported types: Single, Double, Suite." });
        }

        // 8 & 9. Fetch rooms of requested type and validate MaxGuests & Availability
        var candidateRooms = await _dbContext.Rooms
            .Where(r => r.RoomType == normalizedRoomType)
            .ToListAsync(cancellationToken);

        if (!candidateRooms.Any())
        {
            return BadRequest(new { error = $"No rooms found for type '{normalizedRoomType}' in the system." });
        }

        // Check MaxGuests (all rooms of same type have identical MaxGuests)
        var maxCapacity = candidateRooms.First().MaxGuests;
        if (parsedData.Guests > maxCapacity)
        {
            return BadRequest(new { error = $"Guests count ({parsedData.Guests}) exceeds maximum capacity ({maxCapacity}) for {normalizedRoomType} room." });
        }

        // Check available room without overlapping Confirmed bookings:
        // Overlap condition: existing.CheckIn < newCheckOut && existing.CheckOut > newCheckIn
        Room? availableRoom = null;
        foreach (var room in candidateRooms)
        {
            var isBooked = await _dbContext.Bookings.AnyAsync(b =>
                b.RoomId == room.RoomId &&
                b.Status == "Confirmed" &&
                b.CheckIn < checkOut &&
                b.CheckOut > checkIn,
                cancellationToken);

            if (!isBooked)
            {
                availableRoom = room;
                break;
            }
        }

        if (availableRoom == null)
        {
            return Conflict(new { error = $"No {normalizedRoomType} room is available for the requested period from {checkIn:yyyy-MM-dd} to {checkOut:yyyy-MM-dd}." });
        }

        // 10. Calculate TotalPrice
        var nights = checkOut.DayNumber - checkIn.DayNumber;
        var totalPrice = nights * availableRoom.PricePerNight;

        // 11. Save booking to MySQL
        var booking = new Booking
        {
            RoomId = availableRoom.RoomId,
            GuestName = parsedData.GuestName.Trim(),
            Phone = parsedData.Phone.Trim(),
            CheckIn = checkIn,
            CheckOut = checkOut,
            TotalPrice = totalPrice,
            Status = "Confirmed"
        };

        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 12. Return response
        var response = new ParseBookingResponse
        {
            BookingId = booking.BookingId,
            RoomId = availableRoom.RoomId,
            RoomNumber = availableRoom.RoomNumber,
            RoomType = availableRoom.RoomType,
            GuestName = booking.GuestName,
            Phone = booking.Phone,
            CheckIn = checkIn.ToString("yyyy-MM-dd"),
            CheckOut = checkOut.ToString("yyyy-MM-dd"),
            Nights = nights,
            PricePerNight = availableRoom.PricePerNight,
            TotalPrice = totalPrice,
            Status = booking.Status,
            Message = "Booking successfully parsed, validated, and saved to database."
        };

        return Ok(response);
    }
}
