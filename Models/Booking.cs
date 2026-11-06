namespace SUNRISEHOTEL.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public int RoomId { get; set; }

        public string GuestName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public DateOnly CheckIn { get; set; }

        public DateOnly CheckOut { get; set; }

        public decimal TotalPrice { get; set; }

        public string Status { get; set; } = "Confirmed";

        public Room? Room { get; set; }
    }
}
