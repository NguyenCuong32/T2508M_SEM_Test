using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Api.DTOs;
using SunriseHotel.Api.Services;

namespace SunriseHotel.Api.Controllers;

[ApiController]
[Route("api/assistant")]
public class AssistantController : ControllerBase
{
    private readonly GeminiService _geminiService;
    private readonly ILogger<AssistantController> _logger;

    public AssistantController(GeminiService geminiService, ILogger<AssistantController> logger)
    {
        _geminiService = geminiService;
        _logger = logger;
    }

    /// <summary>
    /// Requirement 3: bookingassistant (POST /api/assistant/chat, body: message)
    /// Using Gemini Function Calling to check availability, get bookings, or cancel a booking.
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] AssistantChatRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Message is required." });
        }

        try
        {
            var reply = await _geminiService.ChatAssistantAsync(request.Message);
            return Ok(new AssistantChatResponse { Reply = reply });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Gemini service warning or quota");
            return Ok(new AssistantChatResponse { Reply = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing assistant chat");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi kết nối với trợ lý AI.", details = ex.Message });
        }
    }
}
