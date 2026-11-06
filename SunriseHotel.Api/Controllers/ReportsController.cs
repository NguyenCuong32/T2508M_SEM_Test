using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunriseHotel.Api.Data;
using SunriseHotel.Api.DTOs;

namespace SunriseHotel.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly SunriseHotelDbContext _db;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(SunriseHotelDbContext db, ILogger<ReportsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Requirement 4: getbookingreport (GET /api/reports/bookings?from=...&to=...)
    /// Returns confirmed bookings within date range as a report table.
    /// </summary>
    [HttpGet("bookings")]
    public async Task<IActionResult> GetBookingReport([FromQuery] string? from, [FromQuery] string? to)
    {
        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
        {
            return BadRequest(new { message = "Query parameters 'from' and 'to' (format: yyyy-MM-dd) are required." });
        }

        if (!DateOnly.TryParse(from, out var fromDate) || !DateOnly.TryParse(to, out var toDate))
        {
            return BadRequest(new { message = "Invalid date format. Expected yyyy-MM-dd." });
        }

        if (toDate < fromDate)
        {
            return BadRequest(new { message = "'to' date must be greater than or equal to 'from' date." });
        }

        try
        {
            var query = _db.Bookings
                .Include(b => b.Room)
                .Where(b => b.Status == "Confirmed" 
                            && b.CheckIn >= fromDate 
                            && b.CheckIn <= toDate)
                .OrderBy(b => b.CheckIn)
                .ThenBy(b => b.BookingId);

            var bookings = await query.ToListAsync();

            var report = bookings.Select((b, index) => new BookingReportItemDto
            {
                No = index + 1,
                RoomNumber = b.Room?.RoomNumber ?? "N/A",
                RoomType = b.Room?.RoomType ?? "N/A",
                GuestName = b.GuestName,
                CheckIn = b.CheckIn.ToString("dd/MM/yyyy"),
                CheckOut = b.CheckOut.ToString("dd/MM/yyyy"),
                Nights = b.CheckOut.DayNumber - b.CheckIn.DayNumber,
                TotalPrice = b.TotalPrice
            }).ToList();

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating booking report");
            return StatusCode(500, new { message = "Error retrieving booking report.", details = ex.Message });
        }
    }
}
