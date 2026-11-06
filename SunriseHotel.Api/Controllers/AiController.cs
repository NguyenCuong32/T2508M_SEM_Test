using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Api.DTOs;
using SunriseHotel.Api.Services;

namespace SunriseHotel.Api.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly GeminiService _geminiService;
    private readonly ILogger<AiController> _logger;

    public AiController(GeminiService geminiService, ILogger<AiController> logger)
    {
        _geminiService = geminiService;
        _logger = logger;
    }

    /// <summary>
    /// Requirement 1: askhotel (POST /api/ai/ask, body: question)
    /// Gemini acts as a polite receptionist, only answers questions about Sunrise Hotel.
    /// </summary>
    [HttpPost("ask")]
    public async Task<IActionResult> AskHotel([FromBody] AskHotelRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new { message = "Question is required." });
        }

        try
        {
            var answer = await _geminiService.AskHotelAsync(request.Question);
            return Ok(new AskHotelResponse { Answer = answer });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "API Key or Quota message");
            return Ok(new AskHotelResponse { Answer = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AI ask request");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi kết nối với AI.", details = ex.Message });
        }
    }
}
