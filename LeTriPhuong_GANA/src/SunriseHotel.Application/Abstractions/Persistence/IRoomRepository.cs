using SunriseHotel.Domain.Entities;
using SunriseHotel.Domain.Enums;

namespace SunriseHotel.Application.Abstractions.Persistence;

public interface IRoomRepository
{
    /// <summary>Rooms of the given type with no confirmed booking overlapping [checkIn, checkOut).</summary>
    Task<IReadOnlyList<Room>> GetAvailableRoomsAsync(
        RoomType roomType, DateOnly checkIn, DateOnly checkOut, int minGuests, CancellationToken ct = default);
}
