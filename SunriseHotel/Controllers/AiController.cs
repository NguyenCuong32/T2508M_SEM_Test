using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Models.DTOs;
using SunriseHotel.Services;

namespace SunriseHotel.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly IGeminiService _geminiService;

    public AiController(IGeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    [HttpPost("ask")]
    [ActionName("askhotel")]
    public async Task<IActionResult> AskHotel([FromBody] AskHotelRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new { message = "Vui lòng nhập câu hỏi của quý khách." });
        }

        try
        {
            var answer = await _geminiService.AskHotelAsync(request.Question);
            return Ok(new AskHotelResponse { Answer = answer });
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
            return StatusCode(500, new { message = $"Đã xảy ra lỗi khi xử lý yêu cầu: {ex.Message}" });
        }
    }
}

