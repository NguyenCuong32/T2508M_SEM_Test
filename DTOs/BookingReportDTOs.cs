namespace SunriseHotel.DTOs;

public class BookingReportItemDto
{
    public int No { get; set; }
    public int BookingId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string CheckIn { get; set; } = string.Empty;
    public string CheckOut { get; set; } = string.Empty;
    public int Nights { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class BookingReportResponse
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<BookingReportItemDto> Items { get; set; } = new();
}
