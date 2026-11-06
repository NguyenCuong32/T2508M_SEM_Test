using Microsoft.EntityFrameworkCore;
using SUNRISEHOTEL.Data;
using SUNRISEHOTEL.Models;

namespace SUNRISEHOTEL.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly SunriseHotelDbContext _context;

        public BookingRepository(SunriseHotelDbContext context)
        {
            _context = context;
        }

        public async Task<Booking> CreateAsync(Booking booking)
        {
            _context.Bookings.Add(booking);

            await _context.SaveChangesAsync();

            return booking;
        }

        public async Task<List<Booking>> GetByPhoneAsync(string phone)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .AsNoTracking()
                .Where(b => b.Phone == phone)
                .OrderByDescending(b => b.Id)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(int bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        public async Task<bool> CancelAsync(int bookingId)
        {
            var booking = await GetByIdAsync(bookingId);

            if (booking == null || booking.Status != "Confirmed")
                return false;

            booking.Status = "Cancelled";

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<Booking>> GetReportAsync(
            DateOnly from,
            DateOnly to)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .AsNoTracking()
                .Where(b =>
                    b.Status == "Confirmed" &&
                    b.CheckIn >= from &&
                    b.CheckOut <= to)
                .OrderBy(b => b.CheckIn)
                .ToListAsync();
        }
    }
}
