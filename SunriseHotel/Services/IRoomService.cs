using SunriseHotel.Models;

namespace SunriseHotel.Services;

public interface IRoomService
{
    Task<List<Room>> GetAllRoomsAsync();
    Task<Room?> GetRoomByIdAsync(int roomId);
    Task<List<Room>> GetAvailableRoomsAsync(string roomType, DateOnly checkIn, DateOnly checkOut);
}

