using SunriseHotel.Application.Abstractions.Persistence;
using SunriseHotel.Application.Abstractions.Time;
using SunriseHotel.Application.Common.Exceptions;
using SunriseHotel.Domain.Entities;
using SunriseHotel.Domain.Enums;
using SunriseHotel.Domain.Exceptions;

namespace SunriseHotel.Application.Features.Bookings;

public sealed class BookingService(
    IRoomRepository rooms,
    IBookingRepository bookings,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock) : IBookingService
{
    public async Task<AvailabilityDto> CheckAvailabilityAsync(RoomType roomType, DateOnly checkIn, DateOnly checkOut,
        CancellationToken ct = default)
    {
        var errors = new List<string>();
        BookingRules.EnsureValidStay(checkIn, checkOut, clock.Today, errors);
        BookingRules.ThrowIfAny(errors);

        var available = await rooms.GetAvailableRoomsAsync(roomType, checkIn, checkOut, minGuests: 1, ct);
        var nights = Booking.CalculateNights(checkIn, checkOut);

        var roomDtos = available
            .Select(r => new AvailableRoomDto(r.RoomId, r.RoomNumber, r.RoomType.ToString(), r.PricePerNight,
                r.MaxGuests, r.PricePerNight * nights))
            .ToList();

        return new AvailabilityDto(roomType.ToString(), checkIn, checkOut, nights, roomDtos.Count, roomDtos);
    }

    public async Task<IReadOnlyList<BookingDto>> GetBookingsByPhoneAsync(string phone, CancellationToken ct = default)
    {
        var normalized = BookingRules.NormalizePhone(phone);
        if (!BookingRules.IsValidPhone(normalized))
            throw new ValidationException($"Phone number '{phone}' is invalid.");

        var result = await bookings.GetByPhoneAsync(normalized, ct);
        return result.Select(BookingDto.FromEntity).ToList();
    }

    public async Task<BookingDto> CancelBookingAsync(int bookingId, CancellationToken ct = default)
    {
        var booking = await bookings.GetByIdAsync(bookingId, ct)
                      ?? throw new NotFoundException($"Booking {bookingId} was not found.");
        try
        {
            booking.Cancel();
        }
        catch (DomainException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return BookingDto.FromEntity(booking);
    }

    public async Task<BookingDto> CreateBookingAsync(CreateBookingCommand command, CancellationToken ct = default)
    {
        var available = await rooms.GetAvailableRoomsAsync(
            command.RoomType, command.CheckIn, command.CheckOut, command.Guests, ct);

        var room = available.FirstOrDefault()
                   ?? throw new ConflictException(
                       $"No {command.RoomType} room is available for {command.Guests} guest(s) " +
                       $"from {command.CheckIn:yyyy-MM-dd} to {command.CheckOut:yyyy-MM-dd}.");

        var booking = Booking.Create(room, command.GuestName, command.Phone, command.CheckIn, command.CheckOut);
        await bookings.AddAsync(booking, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return BookingDto.FromEntity(booking);
    }
}
