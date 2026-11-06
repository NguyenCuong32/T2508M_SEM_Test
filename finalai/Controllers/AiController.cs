using Microsoft.AspNetCore.Mvc;
using SunriseHotelAI.DTOs;
using SunriseHotelAI.Services;

namespace SunriseHotelAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly ILogger<AiController> _logger;

    public AiController(IGeminiService geminiService, ILogger<AiController> logger)
    {
        _geminiService = geminiService;
        _logger = logger;
    }

    /// <summary>
    /// Requirement 1: API askhotel (POST /api/ai/ask)
    /// Answers guest questions about Sunrise Hotel as a polite receptionist. Refuses unrelated questions.
    /// </summary>
    [HttpPost("ask")]
    public async Task<IActionResult> AskHotel([FromBody] AskHotelRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new { error = "Câu hỏi (question) không được để trống." });
        }

        try
        {
            var answer = await _geminiService.AskHotelAsync(request.Question);
            return Ok(new AskHotelResponse { Answer = answer });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning("Quota limit reached: {Msg}", ex.Message);
            return StatusCode(429, new { error = "Đã vượt hạn mức yêu cầu Google Gemini API (Quota Exceeded 429). Vui lòng thử lại sau." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AskHotel request");
            return StatusCode(500, new { error = $"Lỗi khi xử lý câu hỏi: {ex.Message}" });
        }
    }
}
