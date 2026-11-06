using Microsoft.AspNetCore.Mvc;
using SunriseHotel.API.DTOs;
using SunriseHotel.API.Services;

namespace SunriseHotel.API.Controllers;

public sealed class HotelController(BookingService bookings) : Controller
{
    [HttpGet("/")]
    public async Task<IActionResult> Index(DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return InvalidRange(from, to, "Ngày lọc không hợp lệ.");

        try
        {
            var rows = await bookings.GetReportAsync(from, to, cancellationToken);
            return View(new BookingReportView(from, to, rows));
        }
        catch (RequestException error) when (error.StatusCode == 400)
        {
            return InvalidRange(from, to, error.Message);
        }
    }

    [HttpGet("/assistant")]
    public IActionResult Assistant() => View();

    private IActionResult InvalidRange(DateOnly? from, DateOnly? to, string error)
    {
        Response.StatusCode = StatusCodes.Status400BadRequest;
        return View(nameof(Index), new BookingReportView(from, to, [], error));
    }
}
