using Microsoft.AspNetCore.Mvc;
using SUNRISEHOTEL.DTOs;
using SUNRISEHOTEL.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SUNRISEHOTEL.Controllers
{
    [Route("api/bookings")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IGeminiService _geminiService;
        private readonly IBookingService _bookingService;

        public BookingsController(
            IGeminiService geminiService,
            IBookingService bookingService)
        {
            _geminiService = geminiService;
            _bookingService = bookingService;
        }

        [HttpPost("parse")]
        public async Task<IActionResult> ParseBooking(
            [FromBody] ParseBookingRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Booking text is required.");

            try
            {
                var bookingRequest =
                    await _geminiService.ParseBookingAsync(request.Text);

                var booking =
                    await _bookingService.CreateBookingAsync(bookingRequest);

                return Ok(new
                {
                    success = true,
                    message = "Booking created successfully.",
                    data = booking
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
