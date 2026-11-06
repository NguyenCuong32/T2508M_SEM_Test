using Microsoft.EntityFrameworkCore;
using SUNRISEHOTEL.Data;
using SUNRISEHOTEL.Models;

namespace SUNRISEHOTEL.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly SunriseHotelDbContext _context;

        public RoomRepository(SunriseHotelDbContext context)
        {
            _context = context;
        }

        public async Task<List<Room>> GetAvailableRoomsAsync(
            string roomType,
            DateOnly checkIn,
            DateOnly checkOut)
        {
            return await _context.Rooms
                .AsNoTracking()
                .Where(r =>
                    r.RoomType == roomType &&
                    !_context.Bookings.Any(b =>
                        b.Id == r.Id &&
                        b.Status == "Confirmed" &&
                        b.CheckIn < checkOut &&
                        b.CheckOut > checkIn
                    )
                )
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();
        }

        public async Task<Room?> GetAvailableRoomAsync(
            string roomType,
            DateOnly checkIn,
            DateOnly checkOut,
            int guests)
        {
            return await _context.Rooms
                .Where(r =>
                    r.RoomType == roomType &&
                    r.MaxGuests >= guests &&
                    !_context.Bookings.Any(b =>
                        b.Id == r.Id &&
                        b.Status == "Confirmed" &&
                        b.CheckIn < checkOut &&
                        b.CheckOut > checkIn
                    )
                )
                .OrderBy(r => r.RoomNumber)
                .FirstOrDefaultAsync();
        }
    }
}
