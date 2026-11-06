using MySqlConnector;
using SunriseHotel.Models;
namespace SunriseHotel.Repositories;

public class BookingRepository(IConfiguration config) : IBookingRepository
{
    MySqlConnection Connection() => new(config.GetConnectionString("Hotel") ?? throw new InvalidOperationException("ConnectionStrings:Hotel missing"));
    static void Param(MySqlCommand c, string n, object v) => c.Parameters.AddWithValue(n, v);
    static DateOnly Date(MySqlDataReader r, int i) => DateOnly.FromDateTime(r.GetDateTime(i));
    public async Task<Booking?> Create(BookingRequest request)
    {
        await using var db = Connection(); await db.OpenAsync(); await using var tx = await db.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        // Lock all rooms of this type in a consistent order before checking overlapping bookings.
        await using var locked = new MySqlCommand("SELECT RoomId,PricePerNight FROM Room WHERE RoomType=@type AND MaxGuests>=@guests ORDER BY RoomId FOR UPDATE", db, tx); Param(locked, "@type", request.RoomType); Param(locked, "@guests", request.Guests);
        var rooms = new List<(int id, decimal price)>(); await using (var reader = await locked.ExecuteReaderAsync()) while (await reader.ReadAsync()) rooms.Add((reader.GetInt32(0), reader.GetDecimal(1)));
        foreach (var room in rooms)
        {
            await using var check = new MySqlCommand("SELECT COUNT(*) FROM Booking WHERE RoomId=@id AND Status='Confirmed' AND CheckIn<@end AND CheckOut>@start", db, tx); Param(check, "@id", room.id); Param(check, "@start", request.CheckIn.ToDateTime(TimeOnly.MinValue)); Param(check, "@end", request.CheckOut.ToDateTime(TimeOnly.MinValue));
            if (Convert.ToInt64(await check.ExecuteScalarAsync()) != 0) continue;
            decimal total = (request.CheckOut.DayNumber - request.CheckIn.DayNumber) * room.price;
            await using var insert = new MySqlCommand("INSERT INTO Booking (RoomId,GuestName,Phone,CheckIn,CheckOut,TotalPrice,Status) VALUES (@id,@name,@phone,@start,@end,@total,'Confirmed')", db, tx); Param(insert, "@id", room.id); Param(insert, "@name", request.GuestName); Param(insert, "@phone", request.Phone); Param(insert, "@start", request.CheckIn.ToDateTime(TimeOnly.MinValue)); Param(insert, "@end", request.CheckOut.ToDateTime(TimeOnly.MinValue)); Param(insert, "@total", total); await insert.ExecuteNonQueryAsync(); int id = (int)insert.LastInsertedId; await tx.CommitAsync(); return new(id, room.id, request.GuestName, request.Phone, request.CheckIn, request.CheckOut, total, "Confirmed");
        }
        await tx.RollbackAsync(); return null;
    }
    public async Task<List<Booking>> ByPhone(string phone) { await using var db = Connection(); await db.OpenAsync(); await using var c = new MySqlCommand("SELECT BookingId,RoomId,GuestName,Phone,CheckIn,CheckOut,TotalPrice,Status FROM Booking WHERE Phone=@phone ORDER BY BookingId DESC", db); Param(c, "@phone", phone); var rows = new List<Booking>(); await using var r = await c.ExecuteReaderAsync(); while (await r.ReadAsync()) rows.Add(new(r.GetInt32(0), r.GetInt32(1), r.GetString(2), r.GetString(3), Date(r, 4), Date(r, 5), r.GetDecimal(6), r.GetString(7))); return rows; }
    public async Task<bool> Cancel(int id) { await using var db = Connection(); await db.OpenAsync(); await using var c = new MySqlCommand("UPDATE Booking SET Status='Cancelled' WHERE BookingId=@id AND Status='Confirmed'", db); Param(c, "@id", id); return await c.ExecuteNonQueryAsync() > 0; }
}
