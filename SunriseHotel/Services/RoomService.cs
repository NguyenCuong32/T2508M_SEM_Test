using Microsoft.EntityFrameworkCore;
using SunriseHotel.Data;
using SunriseHotel.Models;

namespace SunriseHotel.Services;

public class RoomService : IRoomService
{
    private readonly SunriseDbContext _context;

    public RoomService(SunriseDbContext context)
    {
        _context = context;
    }

    public async Task<List<Room>> GetAllRoomsAsync()
    {
        return await _context.Room.AsNoTracking().ToListAsync();
    }

    public async Task<Room?> GetRoomByIdAsync(int roomId)
    {
        return await _context.Room.FirstOrDefaultAsync(r => r.RoomId == roomId);
    }

    public async Task<List<Room>> GetAvailableRoomsAsync(string roomType, DateOnly checkIn, DateOnly checkOut)
    {
        var bookedRoomIds = await _context.Booking
            .Where(b => b.Status == "Confirmed" && b.CheckIn < checkOut && b.CheckOut > checkIn)
            .Select(b => b.RoomId)
            .Distinct()
            .ToListAsync();

        var query = _context.Room.Where(r => !bookedRoomIds.Contains(r.RoomId));

        if (!string.IsNullOrWhiteSpace(roomType))
        {
            query = query.Where(r => r.RoomType.ToLower() == roomType.Trim().ToLower());
        }

        return await query.ToListAsync();
    }
}

