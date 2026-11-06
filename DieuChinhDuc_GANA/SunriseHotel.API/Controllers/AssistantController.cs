using Microsoft.AspNetCore.Mvc;
using SunriseHotel.API.DTOs;
using SunriseHotel.API.Services;

namespace SunriseHotel.API.Controllers;

[ApiController]
[Route("api/assistant")]
public sealed class AssistantController(GeminiService gemini) : ControllerBase
{
    [HttpPost("chat", Name = "bookingassistant")]
    public async Task<AiAnswer> BookingAssistant(
        AssistantChatRequest request, CancellationToken cancellationToken) =>
        new(await gemini.ChatAsync(request.Message, cancellationToken));
}
