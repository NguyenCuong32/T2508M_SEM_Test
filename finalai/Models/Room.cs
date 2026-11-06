namespace SunriseHotelAI.Models;

public class Room
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty; // Single / Double / Suite
    public decimal PricePerNight { get; set; }
    public int MaxGuests { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
