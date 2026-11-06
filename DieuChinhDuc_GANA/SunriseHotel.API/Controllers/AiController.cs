using Microsoft.AspNetCore.Mvc;
using SunriseHotel.API.DTOs;
using SunriseHotel.API.Services;

namespace SunriseHotel.API.Controllers;

[ApiController]
[Route("api/ai")]
public sealed class AiController(GeminiService gemini) : ControllerBase
{
    [HttpPost("ask", Name = "askhotel")]
    public async Task<AiAnswer> AskHotel(AskHotelRequest request, CancellationToken cancellationToken) =>
        new(await gemini.AskHotelAsync(request.Question, cancellationToken));
}
