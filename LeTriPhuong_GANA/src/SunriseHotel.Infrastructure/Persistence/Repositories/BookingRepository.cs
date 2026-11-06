using Microsoft.EntityFrameworkCore;
using SunriseHotel.Application.Abstractions.Persistence;
using SunriseHotel.Domain.Entities;
using SunriseHotel.Domain.Enums;

namespace SunriseHotel.Infrastructure.Persistence.Repositories;

public class BookingRepository(HotelDbContext db) : IBookingRepository
{
    public async Task AddAsync(Booking booking, CancellationToken ct = default) =>
        await db.Bookings.AddAsync(booking, ct);

    public Task<Booking?> GetByIdAsync(int bookingId, CancellationToken ct = default) =>
        db.Bookings.Include(b => b.Room).FirstOrDefaultAsync(b => b.BookingId == bookingId, ct);

    public async Task<IReadOnlyList<Booking>> GetByPhoneAsync(string phone, CancellationToken ct = default) =>
        await db.Bookings.AsNoTracking()
            .Include(b => b.Room)
            .Where(b => b.Phone == phone)
            .OrderByDescending(b => b.CheckIn)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Booking>> GetConfirmedAsync(DateOnly? from, DateOnly? to,
        CancellationToken ct = default)
    {
        var query = db.Bookings.AsNoTracking()
            .Include(b => b.Room)
            .Where(b => b.Status == BookingStatus.Confirmed);

        if (from.HasValue) query = query.Where(b => b.CheckIn >= from.Value);
        if (to.HasValue) query = query.Where(b => b.CheckIn <= to.Value);

        return await query
            .OrderBy(b => b.CheckIn)
            .ThenBy(b => b.Room!.RoomNumber)
            .ToListAsync(ct);
    }
}
