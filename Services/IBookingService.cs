using SUNRISEHOTEL.DTOs;
using SUNRISEHOTEL.Models;

namespace SUNRISEHOTEL.Services
{
    public interface IBookingService
    {
        Task<Booking> CreateBookingAsync(BookingRequestDTO request);

        Task<List<Room>> CheckAvailabilityAsync(
            string roomType,
            DateOnly checkIn,
            DateOnly checkOut);

        Task<List<Booking>> GetBookingsByPhoneAsync(string phone);

        Task<bool> CancelBookingAsync(int bookingId);
    }
}
