using System.Text.Json.Serialization;

namespace SunriseHotelAI.DTOs;

public class AskHotelRequest
{
    public string Question { get; set; } = string.Empty;
}

public class AskHotelResponse
{
    public string Answer { get; set; } = string.Empty;
}

public class ParseBookingRequest
{
    public string Text { get; set; } = string.Empty;
}

public class ParsedBookingData
{
    [JsonPropertyName("guestName")]
    public string GuestName { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("roomType")]
    public string RoomType { get; set; } = string.Empty;

    [JsonPropertyName("checkIn")]
    public string CheckIn { get; set; } = string.Empty;

    [JsonPropertyName("checkOut")]
    public string CheckOut { get; set; } = string.Empty;

    [JsonPropertyName("guests")]
    public int Guests { get; set; }
}

public class BookingResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? BookingId { get; set; }
    public int? RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public string? RoomType { get; set; }
    public string? GuestName { get; set; }
    public string? Phone { get; set; }
    public string? CheckIn { get; set; }
    public string? CheckOut { get; set; }
    public int? Nights { get; set; }
    public decimal? PricePerNight { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? Status { get; set; }
    public ParsedBookingData? ExtractedData { get; set; }
}

public class AssistantChatRequest
{
    public string Message { get; set; } = string.Empty;
    public List<ChatMessageHistory>? History { get; set; }
}

public class ChatMessageHistory
{
    public string Role { get; set; } = "user"; // "user" or "model"
    public string Content { get; set; } = string.Empty;
}

public class AssistantChatResponse
{
    public string Reply { get; set; } = string.Empty;
    public List<FunctionCallLog> ExecutedFunctions { get; set; } = new();
}

public class FunctionCallLog
{
    public string FunctionName { get; set; } = string.Empty;
    public object? Arguments { get; set; }
    public object? Result { get; set; }
}

public class BookingReportItemDto
{
    public int No { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string CheckIn { get; set; } = string.Empty; // dd/MM/yyyy
    public string CheckOut { get; set; } = string.Empty; // dd/MM/yyyy
    public int Nights { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "Confirmed";
}

public class RoomAvailabilityResult
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int MaxGuests { get; set; }
}
