using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunriseHotel.Data;
using SunriseHotel.DTOs;

namespace SunriseHotel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly SunriseHotelDbContext _dbContext;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(SunriseHotelDbContext dbContext, ILogger<ReportsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet("bookings")]
    public async Task<IActionResult> GetBookingReport([FromQuery] string? from, [FromQuery] string? to, CancellationToken cancellationToken)
    {
        // 1. Validate existence of from and to parameters
        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
        {
            return BadRequest(new { error = "Both 'from' and 'to' query parameters are required. Format: YYYY-MM-DD." });
        }

        // 2. Validate date formats
        if (!DateOnly.TryParse(from, out var fromDate) || !DateOnly.TryParse(to, out var toDate))
        {
            return BadRequest(new { error = "Invalid date format. Expected format: YYYY-MM-DD." });
        }

        // 3. Validate from <= to
        if (fromDate > toDate)
        {
            return BadRequest(new { error = "'from' date cannot be after 'to' date." });
        }

        try
        {
            // 4. Query bookings with Status = Confirmed and CheckIn within [fromDate, toDate]
            var bookings = await _dbContext.Bookings
                .Include(b => b.Room)
                .Where(b => b.Status == "Confirmed" && b.CheckIn >= fromDate && b.CheckIn <= toDate)
                .OrderBy(b => b.CheckIn)
                .ThenBy(b => b.BookingId)
                .ToListAsync(cancellationToken);

            // 5. Map to report items and compute nights & sequence number
            var items = bookings.Select((b, index) =>
            {
                var nights = b.CheckOut.DayNumber - b.CheckIn.DayNumber;
                return new BookingReportItemDto
                {
                    No = index + 1,
                    BookingId = b.BookingId,
                    RoomNumber = b.Room != null ? b.Room.RoomNumber : "N/A",
                    RoomType = b.Room != null ? b.Room.RoomType : "N/A",
                    GuestName = b.GuestName,
                    Phone = b.Phone,
                    CheckIn = b.CheckIn.ToString("yyyy-MM-dd"),
                    CheckOut = b.CheckOut.ToString("yyyy-MM-dd"),
                    Nights = nights,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status
                };
            }).ToList();

            var response = new BookingReportResponse
            {
                From = fromDate.ToString("yyyy-MM-dd"),
                To = toDate.ToString("yyyy-MM-dd"),
                TotalBookings = items.Count,
                TotalRevenue = items.Sum(i => i.TotalPrice),
                Items = items
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while generating booking report from {From} to {To}", from, to);
            var msg = ex.InnerException != null ? $"{ex.Message} (Inner: {ex.InnerException.Message})" : ex.Message;
            return StatusCode(500, new { error = msg });
        }
    }
}
