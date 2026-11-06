using SunriseHotel.Domain.Entities;
using SunriseHotel.Domain.Enums;

namespace SunriseHotel.Application.Features.Bookings;

public sealed record CreateBookingCommand(
    string GuestName, string Phone, RoomType RoomType, DateOnly CheckIn, DateOnly CheckOut, int Guests);

public sealed record BookingDto(
    int BookingId, int RoomId, string RoomNumber, string RoomType, string GuestName, string Phone,
    DateOnly CheckIn, DateOnly CheckOut, int Nights, decimal TotalPrice, string Status)
{
    public static BookingDto FromEntity(Booking b) => new(
        b.BookingId, b.RoomId, b.Room?.RoomNumber ?? string.Empty, b.Room?.RoomType.ToString() ?? string.Empty,
        b.GuestName, b.Phone, b.CheckIn, b.CheckOut, b.Nights, b.TotalPrice, b.Status.ToString());
}

public sealed record AvailableRoomDto(
    int RoomId, string RoomNumber, string RoomType, decimal PricePerNight, int MaxGuests, decimal TotalPrice);

public sealed record AvailabilityDto(
    string RoomType, DateOnly CheckIn, DateOnly CheckOut, int Nights, int AvailableCount,
    IReadOnlyList<AvailableRoomDto> Rooms);
