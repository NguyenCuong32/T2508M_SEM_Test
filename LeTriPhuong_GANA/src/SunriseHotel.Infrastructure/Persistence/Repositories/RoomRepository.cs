using Microsoft.EntityFrameworkCore;
using SunriseHotel.Application.Abstractions.Persistence;
using SunriseHotel.Domain.Entities;
using SunriseHotel.Domain.Enums;

namespace SunriseHotel.Infrastructure.Persistence.Repositories;

public class RoomRepository(HotelDbContext db) : IRoomRepository
{
    public async Task<IReadOnlyList<Room>> GetAvailableRoomsAsync(
        RoomType roomType, DateOnly checkIn, DateOnly checkOut, int minGuests, CancellationToken ct = default) =>
        await db.Rooms
            .Where(r => r.RoomType == roomType && r.MaxGuests >= minGuests)
            // Two stays overlap when existing.CheckIn < new.CheckOut AND new.CheckIn < existing.CheckOut
            .Where(r => !r.Bookings.Any(b => b.Status == BookingStatus.Confirmed
                                             && b.CheckIn < checkOut
                                             && checkIn < b.CheckOut))
            .OrderBy(r => r.RoomNumber)
            .ToListAsync(ct);
}
