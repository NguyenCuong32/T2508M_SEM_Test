using Microsoft.AspNetCore.Mvc;
using SUNRISEHOTEL.DTOs;
using SUNRISEHOTEL.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SUNRISEHOTEL.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IBookingRepository _bookingRepository;

        public ReportsController(
            IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetBookingReport(
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to)
        {
            if (to < from)
                return BadRequest("Invalid date range.");

            var bookings = await _bookingRepository.GetReportAsync(
                from, to
            );

            var result = bookings.Select((b, index) =>
                new BookingReportDTO
                {
                    No = index + 1,
                    RoomNumber = b.Room!.RoomNumber,
                    RoomType = b.Room.RoomType,
                    GuestName = b.GuestName,
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                    Nights = b.CheckOut.DayNumber
                             - b.CheckIn.DayNumber,
                    TotalPrice = b.TotalPrice
                }
            ).ToList();

            return Ok(new
            {
                success = true,
                totalBookings = result.Count,
                data = result
            });
        }
    }
}
