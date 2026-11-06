using SUNRISEHOTEL.Models;

namespace SUNRISEHOTEL.Repositories
{
    public interface IRoomRepository
    {
        Task<Room?> GetAvailableRoomAsync(
            string roomType,
            DateOnly checkIn,
            DateOnly checkOut,
            int guests);

        Task<List<Room>> GetAvailableRoomsAsync(
            string roomType,
            DateOnly checkIn,
            DateOnly checkOut);
    }
}
