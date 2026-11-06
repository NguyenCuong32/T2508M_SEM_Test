using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Services;
namespace SunriseHotel.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(IReportService reports) : ControllerBase
{
    [HttpGet("bookings")]
    public async Task<IActionResult> Bookings([FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        if (from == default || to == default || from > to) return BadRequest(new { error = "from và to là bắt buộc; from phải trước hoặc bằng to" });
        return Ok(await reports.GetBookings(from, to));
    }
}
