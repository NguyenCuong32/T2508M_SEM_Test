using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Models.DTOs;
using SunriseHotel.Services;

namespace SunriseHotel.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly IBookingService _bookingService;

    public BookingsController(IGeminiService geminiService, IBookingService bookingService)
    {
        _geminiService = geminiService;
        _bookingService = bookingService;
    }

    [HttpPost("parse")]
    [ActionName("parsebooking")]
    public async Task<IActionResult> ParseBooking([FromBody] ParseBookingRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new { message = "Vui lòng nhập nội dung yêu cầu đặt phòng." });
        }

        try
        {
            var parsed = await _geminiService.ParseBookingAsync(request.Text);
            if (parsed == null)
            {
                return BadRequest(new { message = "AI không thể trích xuất thông tin đặt phòng từ nội dung của bạn. Vui lòng cung cấp rõ họ tên, số điện thoại, loại phòng và ngày nhận/trả phòng." });
            }

            var result = await _bookingService.CreateBookingFromParsedDtoAsync(parsed);
            return Ok(new
            {
                success = true,
                extracted = parsed,
                booking = result
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Đã xảy ra lỗi khi tạo đặt phòng: {ex.Message}" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBookings()
    {
        var bookings = await _bookingService.GetAllBookingsAsync();
        return Ok(bookings);
    }
}

