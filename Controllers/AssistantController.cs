using Microsoft.AspNetCore.Mvc;
using baithi.Models;
using baithi.Services;

namespace baithi.Controllers;

[ApiController]
[Route("api/assistant")]
[Produces("application/json")]
public class AssistantController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly ILogger<AssistantController> _logger;

    public AssistantController(IGeminiService geminiService, ILogger<AssistantController> logger)
    {
        _geminiService = geminiService;
        _logger = logger;
    }

    /// <summary>
    /// Yêu cầu 3: Trợ lý AI thực thi Function Calling (CheckAvailability, GetBookingsByPhone, CancelBooking)
    /// </summary>
    /// <param name="request">Tin nhắn hoặc câu lệnh từ người dùng</param>
    [HttpPost("chat")]
    [ProducesResponseType(typeof(BookingChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Chat([FromBody] BookingChatRequest? request)
    {
        string? message = request?.Message;

        if (string.IsNullOrWhiteSpace(message))
        {
            // Fallback đọc raw body
            using var reader = new StreamReader(Request.Body);
            var raw = await reader.ReadToEndAsync();
            message = raw?.Trim('\"', ' ', '\r', '\n');
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return BadRequest(new { message = "Vui lòng nhập nội dung tin nhắn hoặc yêu cầu cho trợ lý ảo." });
        }

        try
        {
            var response = await _geminiService.BookingAssistantChatAsync(message);
            return Ok(response);
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
            _logger.LogError(ex, "Lỗi xử lý API bookingassistant");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }
}

