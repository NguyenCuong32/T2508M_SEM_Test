using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Models.DTOs;
using SunriseHotel.Services;

namespace SunriseHotel.Controllers;

[ApiController]
[Route("api/config")]
public class ConfigController : ControllerBase
{
    private readonly IGeminiService _geminiService;

    public ConfigController(IGeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            apiKeyStatus = _geminiService.GetApiKeyStatus()
        });
    }

    [HttpPost("key")]
    public IActionResult SetApiKey([FromBody] ApiKeyConfigRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.ApiKey))
        {
            return BadRequest(new { message = "API Key không được để trống." });
        }

        _geminiService.SetApiKey(request.ApiKey);
        return Ok(new
        {
            success = true,
            message = "Đã cập nhật Gemini API Key thành công.",
            apiKeyStatus = _geminiService.GetApiKeyStatus()
        });
    }
}

