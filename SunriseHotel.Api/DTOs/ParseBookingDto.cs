namespace SunriseHotel.Api.DTOs;

public class ParseBookingRequest
{
    public string Text { get; set; } = string.Empty;
}

public class ParsedBookingData
{
    public string GuestName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public string CheckIn { get; set; } = string.Empty;
    public string CheckOut { get; set; } = string.Empty;
    public int Guests { get; set; }
}

public class ParseBookingResponse
{
    public string Message { get; set; } = string.Empty;
    public ParsedBookingData ParsedData { get; set; } = new();
    public object? Booking { get; set; }
}
