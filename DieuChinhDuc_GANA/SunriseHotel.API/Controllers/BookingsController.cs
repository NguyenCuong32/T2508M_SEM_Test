using Microsoft.AspNetCore.Mvc;
using SunriseHotel.API.DTOs;
using SunriseHotel.API.Services;

namespace SunriseHotel.API.Controllers;

[ApiController]
[Route("api/bookings")]
public sealed class BookingsController(GeminiService gemini, BookingService bookings) : ControllerBase
{
    [HttpPost("parse", Name = "parsebooking")]
    public async Task<ActionResult<BookingResult>> ParseBooking(
        ParseBookingRequest request, CancellationToken cancellationToken)
    {
        var parsed = await gemini.ParseBookingAsync(request.Text, cancellationToken);
        var booking = await bookings.CreateAsync(parsed, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, booking);
    }
}
