using SunriseHotelAI.DTOs;

namespace SunriseHotelAI.Services;

public interface IHotelService
{
    Task<List<RoomAvailabilityResult>> CheckAvailabilityAsync(string? roomType, string checkIn, string checkOut);
    Task<List<object>> GetBookingsByPhoneAsync(string phone);
    Task<object> CancelBookingAsync(int bookingId);
    Task<BookingResponse> CreateBookingFromParsedDataAsync(ParsedBookingData data);
    Task<List<BookingReportItemDto>> GetBookingReportAsync(DateOnly? from, DateOnly? to);
}
