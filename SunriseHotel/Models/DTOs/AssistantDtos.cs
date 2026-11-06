namespace SunriseHotel.Models.DTOs;

public class AssistantChatRequest
{
    public string Message { get; set; } = string.Empty;
}

public class AssistantChatResponse
{
    public string Response { get; set; } = string.Empty;
    public string? ToolCalled { get; set; }
    public object? ToolResult { get; set; }
}

public class CheckAvailabilityArgs
{
    public string RoomType { get; set; } = string.Empty;
    public string CheckIn { get; set; } = string.Empty;
    public string CheckOut { get; set; } = string.Empty;
}

public class AvailableRoomDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public string FormattedPrice { get; set; } = string.Empty;
    public int MaxGuests { get; set; }
}

public class PhoneBookingDto
{
    public int BookingId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string CheckIn { get; set; } = string.Empty;
    public string CheckOut { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string FormattedTotalPrice { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class CancelBookingResultDto
{
    public bool Success { get; set; }
    public int BookingId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ApiKeyConfigRequest
{
    public string ApiKey { get; set; } = string.Empty;
}

