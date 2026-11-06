using SunriseHotel.Models;
namespace SunriseHotel.Services;

public interface IBookingService
{
    Task<BookingResult> CreateFromText(string text);
}
public record BookingResult(BookingRequest Parsed, Booking? Booking, string? Error);
