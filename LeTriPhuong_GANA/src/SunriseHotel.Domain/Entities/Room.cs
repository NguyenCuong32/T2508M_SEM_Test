using SunriseHotel.Domain.Enums;

namespace SunriseHotel.Domain.Entities;

public class Room
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public RoomType RoomType { get; set; }
    public decimal PricePerNight { get; set; }
    public int MaxGuests { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public bool CanHost(int guests) => guests > 0 && guests <= MaxGuests;
}
