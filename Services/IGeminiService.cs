using SunriseHotel.Models;
namespace SunriseHotel.Services;

public interface IGeminiService
{
    Task<string> Ask(string question);
    Task<BookingRequest> Parse(string text);
    Task<string> Chat(string message);
}
