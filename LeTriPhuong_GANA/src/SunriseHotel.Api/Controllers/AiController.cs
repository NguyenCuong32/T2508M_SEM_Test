using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Api.Contracts;
using SunriseHotel.Application.Features.HotelQa;

namespace SunriseHotel.Api.Controllers;

[ApiController]
[Route("api/ai")]
[Produces("application/json")]
public class AiController(IHotelQaService hotelQaService) : ControllerBase
{
    /// <summary>Requirement 1 - askhotel: ask the AI receptionist about Sunrise Hotel.</summary>
    [HttpPost("ask", Name = "askhotel")]
    [ProducesResponseType<AskHotelResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AskHotelResponse>> Ask([FromBody] AskHotelRequest request, CancellationToken ct) =>
        Ok(await hotelQaService.AskAsync(request.Question, ct));
}
