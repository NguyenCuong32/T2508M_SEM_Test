using Microsoft.AspNetCore.Mvc;
using baithi.Models;
using baithi.Services;

namespace baithi.Controllers;

[ApiController]
[Route("api/ai")]
[Produces("application/json")]
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
    /// Yêu cầu 1: Hỏi đáp thông tin khách sạn Sunrise Hotel (AI Lễ tân)
    /// </summary>
    /// <param name="request">Chứa trường question hoặc nhận chuỗi text</param>
    [HttpPost("ask")]
    [ProducesResponseType(typeof(AskHotelResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AskHotel([FromBody] AskHotelRequest? request)
    {
        string? question = request?.Question;

        if (string.IsNullOrWhiteSpace(question))
        {
            // Fallback đọc raw body nếu người dùng truyền chuỗi thuần
            using var reader = new StreamReader(Request.Body);
            var raw = await reader.ReadToEndAsync();
            question = raw?.Trim('\"', ' ', '\r', '\n');
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            return BadRequest(new { message = "Vui lòng nhập câu hỏi cho nhân viên lễ tân." });
        }

        try
        {
            var answer = await _geminiService.AskHotelAsync(question);
            return Ok(new AskHotelResponse { Answer = answer });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xử lý API askhotel");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }
}

