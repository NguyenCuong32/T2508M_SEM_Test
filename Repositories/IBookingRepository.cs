using SunriseHotel.Models;

namespace SunriseHotel.Repositories;

public interface IBookingRepository
{
    Task<Booking?> Create(BookingRequest request);
    Task<List<Booking>> ByPhone(string phone);
    Task<bool> Cancel(int id);
}
