namespace SunriseHotel.Models;

public record BookingRequest(string GuestName, string Phone, string RoomType, DateOnly CheckIn, DateOnly CheckOut, int Guests);
