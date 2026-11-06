using System.Text.Json.Serialization;

namespace SunriseHotel.Models.DTOs;

public class ParseBookingRequest
{
    public string Text { get; set; } = string.Empty;
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

public class BookingResponseDto
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
    public decimal TotalPrice { get; set; }
    public string FormattedTotalPrice { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class BookingReportDto
{
    public int No { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string CheckIn { get; set; } = string.Empty;
    public string CheckOut { get; set; } = string.Empty;
    public int Nights { get; set; }
    public decimal TotalPrice { get; set; }
    public string FormattedTotalPrice { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

