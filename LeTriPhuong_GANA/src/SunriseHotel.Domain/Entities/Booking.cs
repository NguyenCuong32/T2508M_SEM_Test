using SunriseHotel.Domain.Enums;
using SunriseHotel.Domain.Exceptions;

namespace SunriseHotel.Domain.Entities;

public class Booking
{
    public int BookingId { get; set; }
    public int RoomId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }

    public Room? Room { get; set; }

    public int Nights => CalculateNights(CheckIn, CheckOut);

    public static int CalculateNights(DateOnly checkIn, DateOnly checkOut) =>
        checkOut.DayNumber - checkIn.DayNumber;

    /// <summary>Factory: creates a confirmed booking and computes TotalPrice = nights x PricePerNight.</summary>
    public static Booking Create(Room room, string guestName, string phone, DateOnly checkIn, DateOnly checkOut)
    {
        var nights = CalculateNights(checkIn, checkOut);
        if (nights <= 0)
            throw new DomainException("CheckOut must be after CheckIn.");

        return new Booking
        {
            RoomId = room.RoomId,
            Room = room,
            GuestName = guestName.Trim(),
            Phone = phone.Trim(),
            CheckIn = checkIn,
            CheckOut = checkOut,
            TotalPrice = nights * room.PricePerNight,
            Status = BookingStatus.Confirmed
        };
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
            throw new DomainException($"Booking {BookingId} is already cancelled.");
        Status = BookingStatus.Cancelled;
    }
}
