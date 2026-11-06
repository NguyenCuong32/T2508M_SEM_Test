using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Application.Features.Reports;

namespace SunriseHotel.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Produces("application/json")]
public class ReportsController(IBookingReportService reportService) : ControllerBase
{
    /// <summary>
    /// Requirement 4 - getbookingreport: confirmed bookings whose check-in date is in [from, to].
    /// Dates: yyyy-MM-dd, both optional.
    /// </summary>
    [HttpGet("bookings", Name = "getbookingreport")]
    [ProducesResponseType<BookingReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BookingReportDto>> GetBookings([FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to, CancellationToken ct) =>
        Ok(await reportService.GetReportAsync(from, to, ct));
}
