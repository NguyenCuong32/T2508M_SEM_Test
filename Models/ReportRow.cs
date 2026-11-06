namespace SunriseHotel.Models;

public record ReportRow(int No, string RoomNumber, string RoomType, string GuestName, DateOnly CheckIn, DateOnly CheckOut, int Nights, decimal TotalPrice);
