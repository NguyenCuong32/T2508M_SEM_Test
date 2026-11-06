using SUNRISEHOTEL.Models;

namespace SUNRISEHOTEL.Repositories
{
    public interface IBookingRepository
    {
        Task<Booking> CreateAsync(Booking booking);

        Task<List<Booking>> GetByPhoneAsync(string phone);

        Task<Booking?> GetByIdAsync(int bookingId);

        Task<bool> CancelAsync(int bookingId);

        Task<List<Booking>> GetReportAsync(
            DateOnly from,
            DateOnly to);
    }
}
