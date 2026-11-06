using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SunriseHotel.DTOs;
using SunriseHotel.Services;

namespace SunriseHotel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssistantController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly IBookingToolService _bookingToolService;
    private readonly ILogger<AssistantController> _logger;

    public AssistantController(
        IGeminiService geminiService,
        IBookingToolService bookingToolService,
        ILogger<AssistantController> logger)
    {
        _geminiService = geminiService;
        _bookingToolService = bookingToolService;
        _logger = logger;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] AssistantChatRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { error = "Message is required." });
        }

        try
        {
            var reply = await _geminiService.ChatAssistantAsync(request.Message, async (fnName, fnArgs) =>
            {
                switch (fnName)
                {
                    case "CheckAvailability":
                    {
                        var roomType = fnArgs.TryGetProperty("roomType", out var rt) ? rt.GetString() ?? "" : "";
                        var checkIn = fnArgs.TryGetProperty("checkIn", out var ci) ? ci.GetString() ?? "" : "";
                        var checkOut = fnArgs.TryGetProperty("checkOut", out var co) ? co.GetString() ?? "" : "";
                        return await _bookingToolService.CheckAvailabilityAsync(roomType, checkIn, checkOut, cancellationToken);
                    }

                    case "GetBookingsByPhone":
                    {
                        var phone = fnArgs.TryGetProperty("phone", out var p) ? p.GetString() ?? "" : "";
                        return await _bookingToolService.GetBookingsByPhoneAsync(phone, cancellationToken);
                    }

                    case "CancelBooking":
                    {
                        int bookingId = 0;
                        if (fnArgs.TryGetProperty("bookingId", out var idElem))
                        {
                            if (idElem.ValueKind == JsonValueKind.Number)
                            {
                                bookingId = idElem.GetInt32();
                            }
                            else if (int.TryParse(idElem.GetString(), out var parsedId))
                            {
                                bookingId = parsedId;
                            }
                        }
                        return await _bookingToolService.CancelBookingAsync(bookingId, cancellationToken);
                    }

                    default:
                        _logger.LogWarning("Unknown function call requested: {FnName}", fnName);
                        return new { success = false, error = $"Unknown function {fnName}" };
                }
            }, cancellationToken);

            return Ok(new AssistantChatResponse { Reply = reply });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error in Assistant Chat");
            return StatusCode(502, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Assistant Chat");
            var msg = ex.InnerException != null ? $"{ex.Message} (Inner: {ex.InnerException.Message})" : ex.Message;
            return StatusCode(500, new { error = msg });
        }
    }
}
