using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Models;
using SunriseHotel.Services;
namespace SunriseHotel.Controllers;

[ApiController]
[Route("api/assistant")]
public class AssistantController(IGeminiService ai) : ControllerBase
{
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message)) return BadRequest(new { error = "message là bắt buộc" });
        return Ok(new { answer = await ai.Chat(request.Message) });
    }
}
