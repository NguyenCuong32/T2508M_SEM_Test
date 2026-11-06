using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Api.DTOs;
using SunriseHotel.Api.Services;

namespace SunriseHotel.Api.Controllers;

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
    /// Requirement 1: API askhotel (POST /api/ai/ask, body: question)
    /// Answers hotel-related questions acting as a polite receptionist, refusing unrelated questions.
    /// </summary>
    [HttpPost("ask")]
    public async Task<IActionResult> AskHotel([FromBody] AskHotelRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new { message = "Vui lòng nhập câu hỏi (field 'question')." });
        }

        try
        {
            var answer = await _geminiService.AskHotelAsync(request.Question.Trim());
            return Ok(new AskHotelResponse { Answer = answer });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Configuration error in AskHotel");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP Error calling Gemini in AskHotel");
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in AskHotel");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Đã xảy ra lỗi nội bộ: " + ex.Message });
        }
    }
}

