namespace SunriseHotel.Models;

public record Room(int RoomId, string RoomNumber, string RoomType, decimal PricePerNight, int MaxGuests);
