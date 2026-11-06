namespace SunriseHotel.Models;

public record Booking(int BookingId, int RoomId, string GuestName, string Phone, DateOnly CheckIn, DateOnly CheckOut, decimal TotalPrice, string Status);
