using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Api.Contracts;
using SunriseHotel.Application.Features.BookingParsing;

namespace SunriseHotel.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public class BookingsController(IBookingParserService bookingParserService) : ControllerBase
{
    /// <summary>Requirement 2 - parsebooking: natural-language booking request -> JSON -> saved booking.</summary>
    [HttpPost("parse", Name = "parsebooking")]
    [ProducesResponseType<ParseBookingResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<ParseBookingResponse>> Parse([FromBody] ParseBookingRequest request,
        CancellationToken ct)
    {
        var result = await bookingParserService.ParseAndCreateAsync(request.Text, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
