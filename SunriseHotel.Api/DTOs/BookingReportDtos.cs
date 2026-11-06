using System.Text.Json.Serialization;

namespace SunriseHotel.Api.DTOs;

public class BookingReportItemDto
{
    [JsonPropertyName("no")]
    public int No { get; set; }

    [JsonPropertyName("roomNumber")]
    public string RoomNumber { get; set; } = string.Empty;

    [JsonPropertyName("roomType")]
    public string RoomType { get; set; } = string.Empty;

    [JsonPropertyName("guestName")]
    public string GuestName { get; set; } = string.Empty;

    [JsonPropertyName("checkIn")]
    public string CheckIn { get; set; } = string.Empty;

    [JsonPropertyName("checkOut")]
    public string CheckOut { get; set; } = string.Empty;

    [JsonPropertyName("nights")]
    public int Nights { get; set; }

    [JsonPropertyName("totalPrice")]
    public decimal TotalPrice { get; set; }
}

