using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Models.DTOs;
using SunriseHotel.Services;

namespace SunriseHotel.Controllers;

[ApiController]
[Route("api/assistant")]
public class AssistantController : ControllerBase
{
    private readonly IGeminiService _geminiService;

    public AssistantController(IGeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    [HttpPost("chat")]
    [ActionName("bookingassistant")]
    public async Task<IActionResult> Chat([FromBody] AssistantChatRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Vui lòng nhập nội dung tin nhắn cần hỗ trợ." });
        }

        try
        {
            var response = await _geminiService.ChatWithAssistantAsync(request.Message, request.History);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Đã xảy ra lỗi khi trao đổi với trợ lý ảo: {ex.Message}" });
        }
    }
}

