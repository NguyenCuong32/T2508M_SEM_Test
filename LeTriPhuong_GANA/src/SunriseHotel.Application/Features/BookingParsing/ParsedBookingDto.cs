using System.Text.Json.Serialization;

namespace SunriseHotel.Application.Features.BookingParsing;

/// <summary>Exact JSON shape returned by Gemini for requirement 2.</summary>
public sealed record ParsedBookingDto
{
    [JsonPropertyName("guestName")] public string? GuestName { get; init; }
    [JsonPropertyName("phone")] public string? Phone { get; init; }
    [JsonPropertyName("roomType")] public string? RoomType { get; init; }
    [JsonPropertyName("checkIn")] public string? CheckIn { get; init; }
    [JsonPropertyName("checkOut")] public string? CheckOut { get; init; }
    [JsonPropertyName("guests")] public int? Guests { get; init; }
}
