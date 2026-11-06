using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunriseHotel.Api.Data;
using SunriseHotel.Api.DTOs;
using SunriseHotel.Api.Models;
using SunriseHotel.Api.Services;

namespace SunriseHotel.Api.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly SunriseHotelDbContext _db;
    private readonly GeminiService _geminiService;
    private readonly ILogger<BookingsController> _logger;

    private static readonly HashSet<string> ValidRoomTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Single", "Double", "Suite"
    };

    public BookingsController(SunriseHotelDbContext db, GeminiService geminiService, ILogger<BookingsController> logger)
    {
        _db = db;
        _geminiService = geminiService;
        _logger = logger;
    }

    /// <summary>
    /// Requirement 2: parsebooking (POST /api/bookings/parse, body: text)
    /// Parses natural language booking request via Gemini JSON mode,
    /// validates dates & roomType, checks availability, calculates TotalPrice,
    /// and saves Confirmed booking to database.
    /// </summary>
    [HttpPost("parse")]
    public async Task<IActionResult> ParseBooking([FromBody] ParseBookingRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new { message = "Booking text is required." });
        }

        try
        {
            // 1. Call Gemini to parse request into structured JSON
            var parsed = await _geminiService.ParseBookingAsync(request.Text);

            // 2. Validate RoomType
            if (string.IsNullOrWhiteSpace(parsed.RoomType) || !ValidRoomTypes.Contains(parsed.RoomType))
            {
                return BadRequest(new
                {
                    message = $"Invalid room type '{parsed.RoomType}'. Valid types are Single, Double, Suite.",
                    parsedData = parsed
                });
            }

            // Normalize roomType case (Single, Double, Suite)
            var normalizedRoomType = ValidRoomTypes.First(v => v.Equals(parsed.RoomType, StringComparison.OrdinalIgnoreCase));
            parsed.RoomType = normalizedRoomType;

            // 3. Validate Dates
            if (!DateOnly.TryParse(parsed.CheckIn, out var checkInDate) ||
                !DateOnly.TryParse(parsed.CheckOut, out var checkOutDate))
            {
                return BadRequest(new
                {
                    message = "Invalid date format parsed from input. Expected YYYY-MM-DD.",
                    parsedData = parsed
                });
            }

            if (checkOutDate <= checkInDate)
            {
                return BadRequest(new
                {
                    message = "CheckOut date must be greater than CheckIn date.",
                    parsedData = parsed
                });
            }

            // 4. Find available room of that type (no overlapping Confirmed booking)
            var bookedRoomIds = await _db.Bookings
                .Where(b => b.Status == "Confirmed"
                            && b.CheckIn < checkOutDate
                            && b.CheckOut > checkInDate)
                .Select(b => b.RoomId)
                .ToListAsync();

            var availableRoom = await _db.Rooms
                .Where(r => r.RoomType == normalizedRoomType && !bookedRoomIds.Contains(r.RoomId))
                .FirstOrDefaultAsync();

            if (availableRoom == null)
            {
                return BadRequest(new
                {
                    message = $"Sorry, no {normalizedRoomType} room is available between {parsed.CheckIn} and {parsed.CheckOut}.",
                    parsedData = parsed
                });
            }

            // 5. Calculate TotalPrice
            var nights = checkOutDate.DayNumber - checkInDate.DayNumber;
            var totalPrice = nights * availableRoom.PricePerNight;

            // 6. Save new booking
            var booking = new Booking
            {
                RoomId = availableRoom.RoomId,
                GuestName = parsed.GuestName,
                Phone = parsed.Phone,
                CheckIn = checkInDate,
                CheckOut = checkOutDate,
                TotalPrice = totalPrice,
                Status = "Confirmed"
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Booking created successfully.",
                booking = new
                {
                    booking.BookingId,
                    booking.RoomId,
                    availableRoom.RoomNumber,
                    availableRoom.RoomType,
                    booking.GuestName,
                    booking.Phone,
                    CheckIn = booking.CheckIn.ToString("yyyy-MM-dd"),
                    CheckOut = booking.CheckOut.ToString("yyyy-MM-dd"),
                    Nights = nights,
                    booking.TotalPrice,
                    booking.Status
                },
                parsedData = parsed
            });
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "JSON parsing error");
            return BadRequest(new { message = "Failed to parse booking from AI output.", error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "API key or service error");
            return StatusCode(500, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in parsebooking");
            return StatusCode(500, new { message = "An error occurred while processing your booking.", details = ex.Message });
        }
    }
}
