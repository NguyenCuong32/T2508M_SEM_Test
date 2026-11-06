using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Api.DTOs;
using SunriseHotel.Api.Services;

namespace SunriseHotel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IHotelBookingService _bookingService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IHotelBookingService bookingService, ILogger<ReportsController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    /// <summary>
    /// Requirement 4: API getbookingreport (GET /api/reports/bookings?from=...&to=...)
    /// Returns confirmed bookings matching the report table format.
    /// </summary>
    [HttpGet("bookings")]
    public async Task<IActionResult> GetBookingReport([FromQuery] string? from, [FromQuery] string? to)
    {
        DateOnly? fromDate = null;
        DateOnly? toDate = null;

        if (!string.IsNullOrWhiteSpace(from))
        {
            if (TryParseDate(from, out var parsedFrom))
            {
                fromDate = parsedFrom;
            }
            else
            {
                return BadRequest(new { message = $"Tham số 'from' không đúng định dạng ngày ({from})." });
            }
        }

        if (!string.IsNullOrWhiteSpace(to))
        {
            if (TryParseDate(to, out var parsedTo))
            {
                toDate = parsedTo;
            }
            else
            {
                return BadRequest(new { message = $"Tham số 'to' không đúng định dạng ngày ({to})." });
            }
        }

        var report = await _bookingService.GetBookingReportAsync(fromDate, toDate);
        return Ok(report);
    }

    private static bool TryParseDate(string input, out DateOnly date)
    {
        date = default;
        string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "yyyy/MM/dd" };
        foreach (var fmt in formats)
        {
            if (DateOnly.TryParseExact(input.Trim(), fmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return true;
            }
        }

        return DateOnly.TryParse(input.Trim(), out date);
    }
}

