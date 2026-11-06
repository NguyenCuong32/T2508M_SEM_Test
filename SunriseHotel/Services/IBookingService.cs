using SunriseHotel.Models;
using SunriseHotel.Models.DTOs;

namespace SunriseHotel.Services;

public interface IBookingService
{
    Task<List<AvailableRoomDto>> CheckAvailabilityAsync(string roomType, string checkInStr, string checkOutStr);
    Task<BookingResponseDto> CreateBookingFromParsedDtoAsync(ParsedBookingDto dto);
    Task<List<PhoneBookingDto>> GetBookingsByPhoneAsync(string phone);
    Task<CancelBookingResultDto> CancelBookingAsync(int bookingId);
    Task<List<BookingReportDto>> GetBookingReportAsync(string? fromDate, string? toDate);
    Task<List<Booking>> GetAllBookingsAsync();
}

