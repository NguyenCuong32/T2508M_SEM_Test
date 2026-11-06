using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Api.DTOs;
using SunriseHotel.Api.Services;

namespace SunriseHotel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly IHotelBookingService _bookingService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(
        IGeminiService geminiService,
        IHotelBookingService bookingService,
        ILogger<BookingsController> logger)
    {
        _geminiService = geminiService;
        _bookingService = bookingService;
        _logger = logger;
    }

    /// <summary>
    /// Requirement 2: API parsebooking (POST /api/bookings/parse, body: text)
    /// Parses natural language booking request via Gemini JSON mode, validates,
    /// finds an available room, calculates TotalPrice and saves booking with status Confirmed.
    /// </summary>
    [HttpPost("parse")]
    public async Task<IActionResult> ParseBooking([FromBody] ParseBookingRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new { message = "Vui lòng nhập nội dung yêu cầu đặt phòng (field 'text')." });
        }

        try
        {
            // 1. Call Gemini with JSON Mode / response schema
            var parsedDto = await _geminiService.ParseBookingAsync(request.Text.Trim());

            // 2. Validate, check availability, calculate price and save to DB
            var (success, message, booking) = await _bookingService.CreateBookingAsync(parsedDto);

            if (!success)
            {
                return BadRequest(new ParseBookingResponse
                {
                    Success = false,
                    Message = message,
                    ParsedData = parsedDto,
                    Booking = null
                });
            }

            return Ok(new ParseBookingResponse
            {
                Success = true,
                Message = message,
                ParsedData = parsedDto,
                Booking = booking
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error in ParseBooking");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP Error calling Gemini in ParseBooking");
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ParseBooking");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Đã xảy ra lỗi nội bộ: " + ex.Message });
        }
    }
}

