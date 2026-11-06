using Microsoft.AspNetCore.Mvc;
using SunriseHotelAI.DTOs;
using SunriseHotelAI.Services;

namespace SunriseHotelAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssistantController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly IHotelService _hotelService;
    private readonly ILogger<AssistantController> _logger;

    public AssistantController(
        IGeminiService geminiService,
        IHotelService hotelService,
        ILogger<AssistantController> logger)
    {
        _geminiService = geminiService;
        _hotelService = hotelService;
        _logger = logger;
    }

    /// <summary>
    /// Requirement 3: API bookingassistant (POST /api/assistant/chat)
    /// AI assistant using Gemini Function Calling to check availability, query bookings by phone, and cancel bookings.
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] AssistantChatRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { error = "Tin nhắn (message) không được để trống." });
        }

        try
        {
            var response = await _geminiService.ChatWithFunctionCallingAsync(
                request.Message,
                request.History,
                _hotelService);

            return Ok(response);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning("Quota limit reached: {Msg}", ex.Message);
            return StatusCode(429, new { error = "Đã vượt hạn mức yêu cầu Google Gemini API (Quota Exceeded 429). Vui lòng thử lại sau." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Assistant Chat request");
            return StatusCode(500, new { error = $"Lỗi xử lý trợ lý: {ex.Message}" });
        }
    }
}
