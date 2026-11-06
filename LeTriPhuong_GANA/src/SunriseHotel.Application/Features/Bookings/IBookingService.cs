using SunriseHotel.Domain.Enums;

namespace SunriseHotel.Application.Features.Bookings;

/// <summary>Core booking use cases, shared by parsebooking (req. 2) and the assistant functions (req. 3).</summary>
public interface IBookingService
{
    Task<AvailabilityDto> CheckAvailabilityAsync(RoomType roomType, DateOnly checkIn, DateOnly checkOut,
        CancellationToken ct = default);

    Task<IReadOnlyList<BookingDto>> GetBookingsByPhoneAsync(string phone, CancellationToken ct = default);
    Task<BookingDto> CancelBookingAsync(int bookingId, CancellationToken ct = default);
    Task<BookingDto> CreateBookingAsync(CreateBookingCommand command, CancellationToken ct = default);
}
