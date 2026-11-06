namespace SunriseHotel.API.Models;

public sealed record Room(int RoomId, string RoomNumber, string RoomType, decimal PricePerNight, int MaxGuests);
