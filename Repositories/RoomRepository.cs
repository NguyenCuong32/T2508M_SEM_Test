using MySqlConnector;
using SunriseHotel.Models;
namespace SunriseHotel.Repositories;

public class RoomRepository(IConfiguration config) : IRoomRepository
{
    MySqlConnection Connection() => new(config.GetConnectionString("Hotel") ?? throw new InvalidOperationException("ConnectionStrings:Hotel missing"));
    static void Param(MySqlCommand c, string n, object v) => c.Parameters.AddWithValue(n, v);
    public async Task<List<Room>> Available(string type, DateOnly start, DateOnly end, int guests = 1)
    {
        await using var db = Connection(); await db.OpenAsync();
        await using var c = new MySqlCommand("SELECT RoomId,RoomNumber,RoomType,PricePerNight,MaxGuests FROM Room r WHERE RoomType=@type AND MaxGuests>=@guests AND NOT EXISTS (SELECT 1 FROM Booking b WHERE b.RoomId=r.RoomId AND b.Status='Confirmed' AND b.CheckIn<@end AND b.CheckOut>@start) ORDER BY RoomId", db);
        Param(c, "@type", type); Param(c, "@guests", guests); Param(c, "@start", start.ToDateTime(TimeOnly.MinValue)); Param(c, "@end", end.ToDateTime(TimeOnly.MinValue));
        var rows = new List<Room>(); await using var reader = await c.ExecuteReaderAsync(); while (await reader.ReadAsync()) rows.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetDecimal(3), reader.GetInt32(4))); return rows;
    }
}
