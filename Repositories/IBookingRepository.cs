using baithi.Models;

namespace baithi.Repositories;

public interface IBookingRepository
{
    Task EnsureDatabaseCreatedAndSeededAsync();
    Task<List<Room>> GetAvailableRoomsAsync(string? roomType, DateOnly checkIn, DateOnly checkOut);
    Task<Room?> GetFirstAvailableRoomAsync(string roomType, DateOnly checkIn, DateOnly checkOut);
    Task<Room?> GetRoomByIdAsync(int roomId);
    Task<Booking> CreateBookingAsync(Booking booking);
    Task<List<Booking>> GetBookingsByPhoneAsync(string phone);
    Task<Booking?> GetBookingByIdAsync(int bookingId);
    Task<Booking?> CancelBookingAsync(int bookingId);
    Task<List<Booking>> GetConfirmedBookingsReportAsync(DateOnly? from, DateOnly? to);
}

