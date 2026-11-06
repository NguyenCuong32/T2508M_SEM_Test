using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using baithi.Models;
using baithi.Repositories;

namespace baithi.Controllers;

[ApiController]
[Route("api/reports")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IBookingRepository _bookingRepo;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IBookingRepository bookingRepo, ILogger<ReportsController> logger)
    {
        _bookingRepo = bookingRepo;
        _logger = logger;
    }

    /// <summary>
    /// Yêu cầu 4: Báo cáo danh sách đơn đặt phòng đã xác nhận (Confirmed) theo khoảng ngày
    /// </summary>
    /// <param name="from">Ngày bắt đầu (hỗ trợ định dạng yyyy-MM-dd hoặc dd/MM/yyyy, để trống nếu lấy tất cả)</param>
    /// <param name="to">Ngày kết thúc (hỗ trợ định dạng yyyy-MM-dd hoặc dd/MM/yyyy, để trống nếu lấy tất cả)</param>
    [HttpGet("bookings")]
    [ProducesResponseType(typeof(List<BookingReportItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBookingReport([FromQuery] string? from, [FromQuery] string? to)
    {
        DateOnly? fromDate = null;
        DateOnly? toDate = null;

        if (!string.IsNullOrWhiteSpace(from))
        {
            fromDate = ParseFlexibleDate(from);
            if (!fromDate.HasValue)
            {
                return BadRequest(new { message = $"Tham số 'from' ({from}) không đúng định dạng. Vui lòng dùng yyyy-MM-dd hoặc dd/MM/yyyy." });
            }
        }

        if (!string.IsNullOrWhiteSpace(to))
        {
            toDate = ParseFlexibleDate(to);
            if (!toDate.HasValue)
            {
                return BadRequest(new { message = $"Tham số 'to' ({to}) không đúng định dạng. Vui lòng dùng yyyy-MM-dd hoặc dd/MM/yyyy." });
            }
        }

        if (fromDate.HasValue && toDate.HasValue && toDate.Value < fromDate.Value)
        {
            return BadRequest(new { message = "Ngày kết thúc 'to' không được nhỏ hơn ngày bắt đầu 'from'." });
        }

        try
        {
            var bookings = await _bookingRepo.GetConfirmedBookingsReportAsync(fromDate, toDate);

            var reportItems = bookings.Select((b, index) =>
            {
                int nights = b.CheckOut.DayNumber - b.CheckIn.DayNumber;
                return new BookingReportItemDto
                {
                    No = index + 1,
                    RoomNumber = b.Room?.RoomNumber ?? "N/A",
                    RoomType = b.Room?.RoomType ?? "N/A",
                    GuestName = b.GuestName,
                    CheckIn = b.CheckIn.ToString("dd/MM/yyyy"),
                    CheckOut = b.CheckOut.ToString("dd/MM/yyyy"),
                    Nights = nights,
                    TotalPrice = b.TotalPrice,
                    TotalPriceFormatted = $"{b.TotalPrice:N0}"
                };
            }).ToList();

            return Ok(reportItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi truy vấn báo cáo đặt phòng");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    private static DateOnly? ParseFlexibleDate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "yyyy/MM/dd", "dd-MM-yyyy" };
        if (DateOnly.TryParseExact(input.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
        {
            return d;
        }
        if (DateOnly.TryParse(input.Trim(), out d))
        {
            return d;
        }
        return null;
    }
}

