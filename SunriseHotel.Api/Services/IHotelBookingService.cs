using SunriseHotel.Api.DTOs;
using SunriseHotel.Api.Models;

namespace SunriseHotel.Api.Services;

public interface IHotelBookingService
{
    Task<List<Room>> CheckAvailabilityAsync(string? roomType, DateOnly checkIn, DateOnly checkOut);
    Task<List<BookingResultDto>> GetBookingsByPhoneAsync(string phone);
    Task<(bool Success, string Message)> CancelBookingAsync(int bookingId);
    Task<(bool Success, string Message, BookingResultDto? Booking)> CreateBookingAsync(ParsedBookingDto dto);
    Task<List<BookingReportItemDto>> GetBookingReportAsync(DateOnly? from, DateOnly? to);
}

