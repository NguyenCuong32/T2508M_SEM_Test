using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using baithi.Models;
using baithi.Repositories;
using baithi.Services;

namespace baithi.Controllers;

[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public class BookingsController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly IBookingRepository _bookingRepo;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(
        IGeminiService geminiService,
        IBookingRepository bookingRepo,
        ILogger<BookingsController> logger)
    {
        _geminiService = geminiService;
        _bookingRepo = bookingRepo;
        _logger = logger;
    }

    /// <summary>
    /// Yêu cầu 2: Bóc tách thông tin đặt phòng tự nhiên bằng Gemini AI và tự động lưu vào CSDL
    /// </summary>
    /// <param name="request">Đoạn văn bản tự nhiên yêu cầu đặt phòng</param>
    [HttpPost("parse")]
    [ProducesResponseType(typeof(ParseBookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ParseBookingResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ParseBooking([FromBody] ParseBookingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Text))
        {
            return BadRequest(new ParseBookingResponse
            {
                Success = false,
                Message = "Vui lòng nhập đoạn văn bản thông tin đặt phòng."
            });
        }

        ParsedBookingDto parsed;
        try
        {
            parsed = await _geminiService.ParseBookingAsync(request.Text);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new ParseBookingResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi Gemini để parse booking");
            return BadRequest(new ParseBookingResponse
            {
                Success = false,
                Message = $"Không thể bóc tách thông tin đặt phòng: {ex.Message}"
            });
        }

        // Validate ngày check-in và check-out
        if (!DateOnly.TryParseExact(parsed.CheckIn, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var checkIn) &&
            !DateOnly.TryParse(parsed.CheckIn, out checkIn))
        {
            return BadRequest(new ParseBookingResponse
            {
                Success = false,
                Message = $"Ngày nhận phòng '{parsed.CheckIn}' không đúng định dạng chuẩn yyyy-MM-dd.",
                ParsedData = parsed
            });
        }

        if (!DateOnly.TryParseExact(parsed.CheckOut, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var checkOut) &&
            !DateOnly.TryParse(parsed.CheckOut, out checkOut))
        {
            return BadRequest(new ParseBookingResponse
            {
                Success = false,
                Message = $"Ngày trả phòng '{parsed.CheckOut}' không đúng định dạng chuẩn yyyy-MM-dd.",
                ParsedData = parsed
            });
        }

        if (checkOut <= checkIn)
        {
            return BadRequest(new ParseBookingResponse
            {
                Success = false,
                Message = $"Ngày trả phòng ({checkOut:dd/MM/yyyy}) phải sau ngày nhận phòng ({checkIn:dd/MM/yyyy}).",
                ParsedData = parsed
            });
        }

        // Validate loại phòng chuẩn
        var normalizedRoomType = NormalizeRoomType(parsed.RoomType);
        if (normalizedRoomType == null)
        {
            return BadRequest(new ParseBookingResponse
            {
                Success = false,
                Message = $"Hạng phòng '{parsed.RoomType}' không hợp lệ. Khách sạn chỉ có 3 hạng phòng: Single, Double, Suite.",
                ParsedData = parsed
            });
        }
        parsed.RoomType = normalizedRoomType;

        // Tìm phòng còn trống
        var availableRoom = await _bookingRepo.GetFirstAvailableRoomAsync(normalizedRoomType, checkIn, checkOut);
        if (availableRoom == null)
        {
            return BadRequest(new ParseBookingResponse
            {
                Success = false,
                Message = $"Rất tiếc, hiện tại không còn phòng {normalizedRoomType} nào trống trong khoảng thời gian từ {checkIn:dd/MM/yyyy} đến {checkOut:dd/MM/yyyy}.",
                ParsedData = parsed
            });
        }

        // Tính số đêm và tổng tiền
        int nights = checkOut.DayNumber - checkIn.DayNumber;
        decimal totalPrice = nights * availableRoom.PricePerNight;

        var booking = new Booking
        {
            RoomId = availableRoom.RoomId,
            GuestName = string.IsNullOrWhiteSpace(parsed.GuestName) ? "Khách Sunrise" : parsed.GuestName.Trim(),
            Phone = string.IsNullOrWhiteSpace(parsed.Phone) ? "Chưa có" : parsed.Phone.Trim(),
            CheckIn = checkIn,
            CheckOut = checkOut,
            TotalPrice = totalPrice,
            Status = "Confirmed"
        };

        try
        {
            var savedBooking = await _bookingRepo.CreateBookingAsync(booking);

            var bookingDto = new BookingDetailsDto
            {
                BookingId = savedBooking.BookingId,
                RoomId = savedBooking.RoomId,
                RoomNumber = availableRoom.RoomNumber,
                RoomType = availableRoom.RoomType,
                GuestName = savedBooking.GuestName,
                Phone = savedBooking.Phone,
                CheckIn = savedBooking.CheckIn.ToString("yyyy-MM-dd"),
                CheckOut = savedBooking.CheckOut.ToString("yyyy-MM-dd"),
                Nights = nights,
                PricePerNight = availableRoom.PricePerNight,
                TotalPrice = savedBooking.TotalPrice,
                TotalPriceFormatted = $"{savedBooking.TotalPrice:N0} VND",
                Status = savedBooking.Status
            };

            return Ok(new ParseBookingResponse
            {
                Success = true,
                Message = $"Đặt phòng thành công! Phòng số {availableRoom.RoomNumber} ({availableRoom.RoomType}) đã được xác nhận.",
                ParsedData = parsed,
                Booking = bookingDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lưu đơn đặt phòng vào cơ sở dữ liệu");
            return StatusCode(StatusCodes.Status500InternalServerError, new ParseBookingResponse
            {
                Success = false,
                Message = $"Lỗi khi lưu thông tin đơn đặt phòng vào cơ sở dữ liệu: {ex.Message}",
                ParsedData = parsed
            });
        }
    }

    private static string? NormalizeRoomType(string? rawType)
    {
        if (string.IsNullOrWhiteSpace(rawType)) return null;

        var clean = rawType.Trim().ToLower();
        if (clean.Contains("suite") || clean.Contains("vip")) return "Suite";
        if (clean.Contains("double") || clean.Contains("đôi") || clean.Contains("doi")) return "Double";
        if (clean.Contains("single") || clean.Contains("đơn") || clean.Contains("don")) return "Single";

        return null;
    }
}

