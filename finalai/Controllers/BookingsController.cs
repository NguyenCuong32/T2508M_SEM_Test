using Microsoft.AspNetCore.Mvc;
using SunriseHotelAI.DTOs;
using SunriseHotelAI.Services;

namespace SunriseHotelAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly IHotelService _hotelService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(
        IGeminiService geminiService,
        IHotelService hotelService,
        ILogger<BookingsController> logger)
    {
        _geminiService = geminiService;
        _hotelService = hotelService;
        _logger = logger;
    }

    /// <summary>
    /// Requirement 2: API parsebooking (POST /api/bookings/parse)
    /// Parses natural language booking request into structured JSON, validates it,
    /// checks room availability, calculates TotalPrice, and saves Confirmed booking.
    /// </summary>
    [HttpPost("parse")]
    public async Task<IActionResult> ParseBooking([FromBody] ParseBookingRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new { error = "Vui lòng cung cấp nội dung đặt phòng (text)." });
        }

        try
        {
            // 1. Call Gemini with JSON Mode / Structured Output
            var parsedData = await _geminiService.ParseBookingAsync(request.Text);

            // 2. Validate, check availability, calculate price, and persist
            var bookingResult = await _hotelService.CreateBookingFromParsedDataAsync(parsedData);

            if (!bookingResult.Success)
            {
                return BadRequest(bookingResult);
            }

            return Ok(bookingResult);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning("Quota limit reached: {Msg}", ex.Message);
            return StatusCode(429, new { error = "Đã vượt hạn mức yêu cầu Google Gemini API (Quota Exceeded 429). Vui lòng thử lại sau." });
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid format or JSON error");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ParseBooking request");
            return StatusCode(500, new { error = $"Lỗi xử lý đặt phòng: {ex.Message}" });
        }
    }
}
