using System.Text.Json;
using SunriseHotel.Api.DTOs;

namespace SunriseHotel.Api.Services;

public interface IGeminiService
{
    Task<string> AskHotelAsync(string question);
    Task<ParsedBookingDto> ParseBookingAsync(string text);
    Task<(string Reply, List<ToolCallLog> ExecutedTools)> ChatWithAssistantAsync(
        string message,
        Func<string, JsonElement, Task<object>> toolExecutor);
}

