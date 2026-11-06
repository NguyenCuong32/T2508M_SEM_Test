using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SunriseHotel.Api.DTOs;
using SunriseHotel.Api.Services;

namespace SunriseHotel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssistantController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly IHotelBookingService _bookingService;
    private readonly ILogger<AssistantController> _logger;

    public AssistantController(
        IGeminiService geminiService,
        IHotelBookingService bookingService,
        ILogger<AssistantController> logger)
    {
        _geminiService = geminiService;
        _bookingService = bookingService;
        _logger = logger;
    }

    /// <summary>
    /// Requirement 3: API bookingassistant (POST /api/assistant/chat, body: message)
    /// Using Gemini Function Calling to execute C# methods: CheckAvailability, GetBookingsByPhone, CancelBooking.
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] AssistantChatRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Vui lòng nhập tin nhắn (field 'message')." });
        }

        try
        {
            var (reply, executedTools) = await _geminiService.ChatWithAssistantAsync(
                request.Message.Trim(),
                ExecuteToolAsync);

            return Ok(new AssistantChatResponse
            {
                Reply = reply,
                ExecutedTools = executedTools
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error in Assistant Chat");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP Error calling Gemini in Assistant Chat");
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Assistant Chat");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Đã xảy ra lỗi nội bộ: " + ex.Message });
        }
    }

    private async Task<object> ExecuteToolAsync(string functionName, JsonElement args)
    {
        switch (functionName)
        {
            case "CheckAvailability":
            {
                string? roomType = args.TryGetProperty("roomType", out var rt) ? rt.GetString() : null;
                string? inStr = args.TryGetProperty("checkIn", out var ci) ? ci.GetString() : null;
                string? outStr = args.TryGetProperty("checkOut", out var co) ? co.GetString() : null;

                if (!TryParseDate(inStr, out var checkIn) || !TryParseDate(outStr, out var checkOut))
                {
                    return new { error = "Ngày nhận phòng hoặc trả phòng không đúng định dạng YYYY-MM-DD." };
                }

                if (checkOut <= checkIn)
                {
                    return new { error = "Ngày trả phòng phải sau ngày nhận phòng." };
                }

                var rooms = await _bookingService.CheckAvailabilityAsync(roomType, checkIn, checkOut);
                int nights = checkOut.DayNumber - checkIn.DayNumber;

                return new
                {
                    requestedRoomType = roomType ?? "All",
                    checkIn = checkIn.ToString("dd/MM/yyyy"),
                    checkOut = checkOut.ToString("dd/MM/yyyy"),
                    nights,
                    availableRoomsCount = rooms.Count,
                    rooms = rooms.Select(r => new
                    {
                        r.RoomNumber,
                        r.RoomType,
                        r.PricePerNight,
                        totalPriceForStay = r.PricePerNight * nights,
                        r.MaxGuests
                    })
                };
            }

            case "GetBookingsByPhone":
            {
                string phone = args.TryGetProperty("phone", out var p) ? p.GetString() ?? "" : "";
                if (string.IsNullOrWhiteSpace(phone))
                {
                    return new { error = "Vui lòng cung cấp số điện thoại." };
                }

                var bookings = await _bookingService.GetBookingsByPhoneAsync(phone);
                return new
                {
                    phone,
                    bookingsCount = bookings.Count,
                    bookings
                };
            }

            case "CancelBooking":
            {
                int bookingId = 0;
                if (args.TryGetProperty("bookingId", out var idElem))
                {
                    if (idElem.ValueKind == JsonValueKind.Number)
                    {
                        bookingId = idElem.GetInt32();
                    }
                    else if (idElem.ValueKind == JsonValueKind.String && int.TryParse(idElem.GetString(), out var parsedId))
                    {
                        bookingId = parsedId;
                    }
                }

                if (bookingId <= 0)
                {
                    return new { error = "Mã booking không hợp lệ." };
                }

                var (success, msg) = await _bookingService.CancelBookingAsync(bookingId);
                return new
                {
                    success,
                    bookingId,
                    message = msg
                };
            }

            default:
                return new { error = $"Không tìm thấy function {functionName}." };
        }
    }

    private static bool TryParseDate(string? input, out DateOnly date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(input)) return false;

        string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "yyyy/MM/dd" };
        foreach (var fmt in formats)
        {
            if (DateOnly.TryParseExact(input.Trim(), fmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return true;
            }
        }

        return DateOnly.TryParse(input.Trim(), out date);
    }
}

