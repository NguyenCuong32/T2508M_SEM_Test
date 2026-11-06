using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Services;

namespace SunriseHotel.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public ReportsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("bookings")]
    [ActionName("getbookingreport")]
    public async Task<IActionResult> GetBookingReport([FromQuery] string? from, [FromQuery] string? to)
    {
        try
        {
            var report = await _bookingService.GetBookingReportAsync(from, to);
            return Ok(report);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Đã xảy ra lỗi khi tạo báo cáo đặt phòng: {ex.Message}" });
        }
    }
}

