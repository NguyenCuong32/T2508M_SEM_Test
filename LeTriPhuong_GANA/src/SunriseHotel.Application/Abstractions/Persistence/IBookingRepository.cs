using SunriseHotel.Domain.Entities;

namespace SunriseHotel.Application.Abstractions.Persistence;

public interface IBookingRepository
{
    Task AddAsync(Booking booking, CancellationToken ct = default);
    Task<Booking?> GetByIdAsync(int bookingId, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetByPhoneAsync(string phone, CancellationToken ct = default);

    /// <summary>Confirmed bookings whose check-in date is within [from, to] (both optional).</summary>
    Task<IReadOnlyList<Booking>> GetConfirmedAsync(DateOnly? from, DateOnly? to, CancellationToken ct = default);
}
