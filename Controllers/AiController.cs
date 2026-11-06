using Microsoft.AspNetCore.Mvc;
using SunriseHotel.DTOs;
using SunriseHotel.Services;

namespace SunriseHotel.Controllers;

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

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] AskHotelRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new { error = "Question is required." });
        }

        try
        {
            var answer = await _geminiService.AskHotelAsync(request.Question, cancellationToken);
            return Ok(new AskHotelResponse { Answer = answer });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Configuration error in AskHotel");
            return StatusCode(500, new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error communicating with Gemini");
            return StatusCode(502, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in AskHotel");
            return StatusCode(500, new { error = "An internal error occurred while processing your question." });
        }
    }
}
