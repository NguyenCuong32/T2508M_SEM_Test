using Microsoft.AspNetCore.Mvc;
using SunriseHotel.API.DTOs;
using SunriseHotel.API.Services;

namespace SunriseHotel.API.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController(BookingService bookings) : ControllerBase
{
    [HttpGet("bookings", Name = "getbookingreport")]
    public async Task<ActionResult<IReadOnlyList<BookingReportRow>>> GetBookingReport(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken) =>
        Ok(await bookings.GetReportAsync(from, to, cancellationToken));
}
