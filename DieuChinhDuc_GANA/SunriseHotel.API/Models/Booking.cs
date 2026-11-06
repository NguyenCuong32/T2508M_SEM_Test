namespace SunriseHotel.API.Models;

public sealed record Booking(
    int BookingId, int RoomId, string GuestName, string Phone,
    DateOnly CheckIn, DateOnly CheckOut, decimal TotalPrice, string Status);
