namespace SUNRISEHOTEL.Models
{
    public class Room
    {
        public int Id { get; set; }

        public string RoomNumber { get; set; } = string.Empty;

        public string RoomType { get; set; } = string.Empty;

        public decimal PricePerNight { get; set; }

        public int MaxGuests { get; set; }

        public ICollection<Booking> Bookings { get; set; }
            = new List<Booking>();
    }
}
