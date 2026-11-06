using System.Text.Json;
using SunriseHotel.DTOs;

namespace SunriseHotel.Services;

public interface IGeminiService
{
    Task<string> AskHotelAsync(string question, CancellationToken cancellationToken = default);
    Task<ParsedBookingData?> ParseBookingAsync(string text, CancellationToken cancellationToken = default);
    Task<string> ChatAssistantAsync(string userMessage, Func<string, JsonElement, Task<object>> executeToolFunc, CancellationToken cancellationToken = default);
}
