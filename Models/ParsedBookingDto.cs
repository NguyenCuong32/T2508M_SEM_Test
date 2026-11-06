namespace SunriseHotel.Models;
// Intermediate JSON from Gemini: validate date strings before constructing the domain request.
public record ParsedBookingDto(string? GuestName, string? Phone, string? RoomType, string? CheckIn, string? CheckOut, int Guests);
