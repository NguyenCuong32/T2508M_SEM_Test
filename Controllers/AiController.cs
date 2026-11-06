using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Models;
using SunriseHotel.Services;
namespace SunriseHotel.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController(IGeminiService ai) : ControllerBase
{
    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] QuestionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question)) return BadRequest(new { error = "question là bắt buộc" });
        return Ok(new { answer = await ai.Ask(request.Question) });
    }
}
