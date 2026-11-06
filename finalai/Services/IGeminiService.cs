using SunriseHotelAI.DTOs;

namespace SunriseHotelAI.Services;

public interface IGeminiService
{
    Task<string> AskHotelAsync(string question);
    Task<ParsedBookingData> ParseBookingAsync(string text);
    Task<AssistantChatResponse> ChatWithFunctionCallingAsync(string message, List<ChatMessageHistory>? history, IHotelService hotelService);
    bool IsConfigured();
}
