namespace SUNRISEHOTEL.DTOs
{
    public class BookingRequestDTO
    {
        public string GuestName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string RoomType { get; set; } = string.Empty;

        public DateOnly CheckIn { get; set; }

        public DateOnly CheckOut { get; set; }

        public int Guests { get; set; }
    }
}
