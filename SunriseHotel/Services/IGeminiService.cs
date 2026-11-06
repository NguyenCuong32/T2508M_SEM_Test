using SunriseHotel.Models.DTOs;

namespace SunriseHotel.Services;

public interface IGeminiService
{
    Task<string> AskHotelAsync(string question);
    Task<ParsedBookingDto?> ParseBookingAsync(string text);
    Task<AssistantChatResponse> ChatWithAssistantAsync(string message, List<ChatHistoryItem>? history = null);
    string GetApiKeyStatus();
    void SetApiKey(string apiKey);
}

