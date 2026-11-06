using Microsoft.AspNetCore.Mvc;
using SunriseHotelAI.DTOs;
using SunriseHotelAI.Services;

namespace SunriseHotelAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IHotelService _hotelService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IHotelService hotelService, ILogger<ReportsController> logger)
    {
        _hotelService = hotelService;
        _logger = logger;
    }

    /// <summary>
    /// Requirement 4: API getbookingreport (GET /api/reports/bookings?from=...&to=...)
    /// Returns confirmed bookings report filtered by date range.
    /// </summary>
    [HttpGet("bookings")]
    public async Task<IActionResult> GetBookingReport([FromQuery] string? from, [FromQuery] string? to)
    {
        DateOnly? fromDate = null;
        DateOnly? toDate = null;

        if (!string.IsNullOrWhiteSpace(from))
        {
            if (HotelService.TryParseDate(from, out var f))
            {
                fromDate = f;
            }
            else
            {
                return BadRequest(new { error = $"Tham số ngày bắt đầu 'from={from}' không hợp lệ (hỗ trợ YYYY-MM-DD hoặc dd/MM/yyyy)." });
            }
        }

        if (!string.IsNullOrWhiteSpace(to))
        {
            if (HotelService.TryParseDate(to, out var t))
            {
                toDate = t;
            }
            else
            {
                return BadRequest(new { error = $"Tham số ngày kết thúc 'to={to}' không hợp lệ (hỗ trợ YYYY-MM-DD hoặc dd/MM/yyyy)." });
            }
        }

        if (fromDate.HasValue && toDate.HasValue && toDate < fromDate)
        {
            return BadRequest(new { error = "Ngày kết thúc 'to' không được nhỏ hơn ngày bắt đầu 'from'." });
        }

        try
        {
            var report = await _hotelService.GetBookingReportAsync(fromDate, toDate);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating booking report");
            return StatusCode(500, new { error = $"Lỗi tạo báo cáo: {ex.Message}" });
        }
    }
}
