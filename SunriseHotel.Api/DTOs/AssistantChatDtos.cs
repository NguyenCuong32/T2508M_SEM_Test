using System.Text.Json.Serialization;

namespace SunriseHotel.Api.DTOs;

public class AssistantChatRequest
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public class AssistantChatResponse
{
    [JsonPropertyName("reply")]
    public string Reply { get; set; } = string.Empty;

    [JsonPropertyName("executedTools")]
    public List<ToolCallLog> ExecutedTools { get; set; } = new();
}

public class ToolCallLog
{
    public string FunctionName { get; set; } = string.Empty;
    public object? Arguments { get; set; }
    public object? Result { get; set; }
}

