namespace SunriseHotel.Services;

public interface IBookingToolService
{
    Task<object> CheckAvailabilityAsync(string roomType, string checkIn, string checkOut, CancellationToken cancellationToken = default);
    Task<object> GetBookingsByPhoneAsync(string phone, CancellationToken cancellationToken = default);
    Task<object> CancelBookingAsync(int bookingId, CancellationToken cancellationToken = default);
}
