namespace SUNRISEHOTEL.DTOs
{
    public class BookingReportDTO
    {
        public int No { get; set; }

        public string RoomNumber { get; set; } = string.Empty;

        public string RoomType { get; set; } = string.Empty;

        public string GuestName { get; set; } = string.Empty;

        public DateOnly CheckIn { get; set; }

        public DateOnly CheckOut { get; set; }

        public int Nights { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
