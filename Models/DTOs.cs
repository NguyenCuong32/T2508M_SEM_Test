using System.Text.Json.Serialization;

namespace baithi.Models;

public class AskHotelRequest
{
    public string? Question { get; set; }
}

public class AskHotelResponse
{
    public string Answer { get; set; } = string.Empty;
}

public class ParseBookingRequest
{
    public string? Text { get; set; }
}

public class ParsedBookingDto
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

public class BookingDetailsDto
{
    public int BookingId { get; set; }
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string CheckIn { get; set; } = string.Empty;
    public string CheckOut { get; set; } = string.Empty;
    public int Nights { get; set; }
    public decimal PricePerNight { get; set; }
    public decimal TotalPrice { get; set; }
    public string TotalPriceFormatted { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class ParseBookingResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ParsedBookingDto? ParsedData { get; set; }
    public BookingDetailsDto? Booking { get; set; }
}

public class BookingChatRequest
{
    public string? Message { get; set; }
}

public class BookingChatResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<string> ExecutedFunctions { get; set; } = new();
}

public class BookingReportItemDto
{
    public int No { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string CheckIn { get; set; } = string.Empty;
    public string CheckOut { get; set; } = string.Empty;
    public int Nights { get; set; }
    public decimal TotalPrice { get; set; }
    public string TotalPriceFormatted { get; set; } = string.Empty;
}

