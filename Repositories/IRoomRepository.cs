using SunriseHotel.Models;
namespace SunriseHotel.Repositories;

public interface IRoomRepository { Task<List<Room>> Available(string type, DateOnly start, DateOnly end, int guests = 1); }
