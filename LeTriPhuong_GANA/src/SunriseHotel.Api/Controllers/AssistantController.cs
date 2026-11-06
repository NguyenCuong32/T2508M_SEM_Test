using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Api.Contracts;
using SunriseHotel.Application.Features.Assistant;

namespace SunriseHotel.Api.Controllers;

[ApiController]
[Route("api/assistant")]
[Produces("application/json")]
public class AssistantController(IBookingAssistantService assistantService) : ControllerBase
{
    /// <summary>Requirement 3 - bookingassistant: chat with the assistant (Gemini Function Calling).</summary>
    [HttpPost("chat", Name = "bookingassistant")]
    [ProducesResponseType<AssistantChatResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AssistantChatResponse>> Chat([FromBody] AssistantChatRequest request,
        CancellationToken ct) =>
        Ok(await assistantService.ChatAsync(request.Message, ct));
}
