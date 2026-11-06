using MySqlConnector;
using SunriseHotel.Models;
namespace SunriseHotel.Repositories;

public class ReportRepository(IConfiguration config) : IReportRepository
{
    MySqlConnection Connection() => new(config.GetConnectionString("Hotel") ?? throw new InvalidOperationException("ConnectionStrings:Hotel missing"));
    static void Param(MySqlCommand c, string n, object v) => c.Parameters.AddWithValue(n, v);
    static DateOnly Date(MySqlDataReader r, int i) => DateOnly.FromDateTime(r.GetDateTime(i));
    public async Task<List<ReportRow>> Report(DateOnly from, DateOnly to) { await using var db = Connection(); await db.OpenAsync(); await using var c = new MySqlCommand("SELECT r.RoomNumber,r.RoomType,b.GuestName,b.CheckIn,b.CheckOut,b.TotalPrice FROM Booking b JOIN Room r ON r.RoomId=b.RoomId WHERE b.Status='Confirmed' AND b.CheckIn>=@from AND b.CheckIn<=@to ORDER BY b.CheckIn,b.BookingId", db); Param(c, "@from", from.ToDateTime(TimeOnly.MinValue)); Param(c, "@to", to.ToDateTime(TimeOnly.MinValue)); var rows = new List<ReportRow>(); await using var r = await c.ExecuteReaderAsync(); while (await r.ReadAsync()) { var start = Date(r, 3); var end = Date(r, 4); rows.Add(new(rows.Count + 1, r.GetString(0), r.GetString(1), r.GetString(2), start, end, end.DayNumber - start.DayNumber, r.GetDecimal(5))); } return rows; }
}
