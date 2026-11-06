using System.Data;
using MySqlConnector;
using SunriseHotel.API.DTOs;
using SunriseHotel.API.Models;

namespace SunriseHotel.API.Data;

public sealed class BookingRepository(MySqlDataSource dataSource)
{
    public async Task<IReadOnlyList<Room>> CheckAvailabilityAsync(
        string roomType, DateOnly checkIn, DateOnly checkOut, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await ReadAvailableRoomsAsync(connection, null, roomType, checkIn, checkOut, 1, cancellationToken);
    }

    public async Task<BookingResult?> CreateAsync(
        string guestName, string phone, string roomType, DateOnly checkIn, DateOnly checkOut,
        int guests, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await using (var lockRooms = new MySqlCommand("""
            SELECT RoomId FROM Room WHERE RoomType = @roomType ORDER BY RoomId FOR UPDATE
            """, connection, transaction))
        {
            lockRooms.Parameters.AddWithValue("@roomType", roomType);
            await using var reader = await lockRooms.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
            }
        }

        var available = await ReadAvailableRoomsAsync(
            connection, transaction, roomType, checkIn, checkOut, guests, cancellationToken);
        if (available.Count == 0)
            return null;

        var room = available[0];
        var nights = checkOut.DayNumber - checkIn.DayNumber;
        var totalPrice = nights * room.PricePerNight;
        await using var insert = new MySqlCommand("""
            INSERT INTO Booking (RoomId, GuestName, Phone, CheckIn, CheckOut, TotalPrice, Status)
            VALUES (@roomId, @name, @phone, @checkIn, @checkOut, @totalPrice, 'Confirmed')
            """, connection, transaction);
        insert.Parameters.AddWithValue("@roomId", room.RoomId);
        insert.Parameters.AddWithValue("@name", guestName);
        insert.Parameters.AddWithValue("@phone", phone);
        insert.Parameters.AddWithValue("@checkIn", checkIn);
        insert.Parameters.AddWithValue("@checkOut", checkOut);
        insert.Parameters.AddWithValue("@totalPrice", totalPrice);
        await insert.ExecuteNonQueryAsync(cancellationToken);
        var bookingId = checked((int)insert.LastInsertedId);

        await transaction.CommitAsync(cancellationToken);
        return new BookingResult(bookingId, room.RoomId, room.RoomNumber, room.RoomType,
            guestName, phone, checkIn, checkOut, guests, nights, totalPrice, "Confirmed");
    }

    public async Task<IReadOnlyList<Booking>> GetByPhoneAsync(string phone, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("""
            SELECT BookingId, RoomId, GuestName, Phone, CheckIn, CheckOut, TotalPrice, Status
            FROM Booking WHERE Phone = @phone ORDER BY CheckIn DESC, BookingId DESC
            """, connection);
        command.Parameters.AddWithValue("@phone", phone);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var bookings = new List<Booking>();
        while (await reader.ReadAsync(cancellationToken))
        {
            bookings.Add(new Booking(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2),
                reader.GetString(3), reader.GetDateOnly(4), reader.GetDateOnly(5),
                reader.GetDecimal(6), reader.GetString(7)));
        }

        return bookings;
    }

    public async Task<string?> CancelAsync(int bookingId, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("""
            UPDATE Booking SET Status = 'Cancelled'
            WHERE BookingId = @bookingId AND Status = 'Confirmed'
            """, connection);
        command.Parameters.AddWithValue("@bookingId", bookingId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 1)
            return "Cancelled";

        command.CommandText = "SELECT Status FROM Booking WHERE BookingId = @bookingId";
        return await command.ExecuteScalarAsync(cancellationToken) as string;
    }

    public async Task<IReadOnlyList<BookingReportRow>> GetReportAsync(
        DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("""
            SELECT r.RoomNumber, r.RoomType, b.GuestName, b.CheckIn, b.CheckOut,
                   DATEDIFF(b.CheckOut, b.CheckIn), b.TotalPrice
            FROM Booking AS b INNER JOIN Room AS r ON b.RoomId = r.RoomId
            WHERE b.Status = 'Confirmed'
              AND (@from IS NULL OR b.CheckIn >= @from)
              AND (@to IS NULL OR b.CheckIn <= @to)
            ORDER BY b.CheckIn, b.BookingId
            """, connection);
        command.Parameters.AddWithValue("@from", from);
        command.Parameters.AddWithValue("@to", to);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var rows = new List<BookingReportRow>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new BookingReportRow(rows.Count + 1, reader.GetString(0), reader.GetString(1),
                reader.GetString(2), reader.GetDateOnly(3), reader.GetDateOnly(4),
                reader.GetInt32(5), reader.GetDecimal(6)));
        }

        return rows;
    }

    private static async Task<IReadOnlyList<Room>> ReadAvailableRoomsAsync(
        MySqlConnection connection, MySqlTransaction? transaction, string roomType,
        DateOnly checkIn, DateOnly checkOut, int guests, CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand("""
            SELECT r.RoomId, r.RoomNumber, r.RoomType, r.PricePerNight, r.MaxGuests
            FROM Room AS r
            WHERE r.RoomType = @roomType AND r.MaxGuests >= @guests
              AND NOT EXISTS (
                  SELECT 1 FROM Booking AS b
                  WHERE b.RoomId = r.RoomId AND b.Status = 'Confirmed'
                    AND b.CheckIn < @checkOut AND b.CheckOut > @checkIn
              )
            ORDER BY r.RoomId
            """, connection, transaction);
        command.Parameters.AddWithValue("@roomType", roomType);
        command.Parameters.AddWithValue("@guests", guests);
        command.Parameters.AddWithValue("@checkIn", checkIn);
        command.Parameters.AddWithValue("@checkOut", checkOut);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var rooms = new List<Room>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rooms.Add(new Room(reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
                reader.GetDecimal(3), reader.GetInt32(4)));
        }

        return rooms;
    }
}
