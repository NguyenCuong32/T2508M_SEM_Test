using baithi.Models;

namespace baithi.Services;

public interface IGeminiService
{
    Task<string> AskHotelAsync(string question);
    Task<ParsedBookingDto> ParseBookingAsync(string text);
    Task<BookingChatResponse> BookingAssistantChatAsync(string message);
}

