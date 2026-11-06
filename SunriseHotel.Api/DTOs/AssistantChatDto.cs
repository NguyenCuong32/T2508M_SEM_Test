namespace SunriseHotel.Api.DTOs;

public class AssistantChatRequest
{
    public string Message { get; set; } = string.Empty;
}

public class AssistantChatResponse
{
    public string Reply { get; set; } = string.Empty;
}
