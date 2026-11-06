using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Models;
using SunriseHotel.Services;
namespace SunriseHotel.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController(IBookingService bookings) : ControllerBase
{
    [HttpPost("parse")]
    public async Task<IActionResult> Parse([FromBody] TextRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text)) return BadRequest(new { error = "text là bắt buộc" });
        var result = await bookings.CreateFromText(request.Text);
        if (result.Error is not null) return BadRequest(new { error = result.Error, parsed = result.Parsed });
        if (result.Booking is null) return Conflict(new { error = "Không có phòng phù hợp trong khoảng thời gian này.", parsed = result.Parsed });
        return Ok(new { parsed = result.Parsed, booking = result.Booking });
    }
}
