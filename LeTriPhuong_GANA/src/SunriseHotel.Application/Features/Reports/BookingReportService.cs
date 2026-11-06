using System.Globalization;
using SunriseHotel.Application.Abstractions.Persistence;
using SunriseHotel.Application.Common.Exceptions;

namespace SunriseHotel.Application.Features.Reports;

/// <summary>One row of the booking report (requirement 4).</summary>
public sealed record BookingReportRowDto(
    int No,
    string RoomNumber,
    string RoomType,
    string GuestName,
    string CheckIn,
    string CheckOut,
    int Nights,
    string TotalPrice,
    decimal TotalPriceValue);

public sealed record BookingReportDto(
    DateOnly? From, DateOnly? To, int TotalBookings, string GrandTotal, IReadOnlyList<BookingReportRowDto> Rows);

public interface IBookingReportService
{
    Task<BookingReportDto> GetReportAsync(DateOnly? from, DateOnly? to, CancellationToken ct = default);
}

public sealed class BookingReportService(IBookingRepository bookings) : IBookingReportService
{
    private static readonly CultureInfo Vietnamese = CultureInfo.GetCultureInfo("vi-VN");
    private const string DateFormat = "dd/MM/yyyy";

    public async Task<BookingReportDto> GetReportAsync(DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        if (from.HasValue && to.HasValue && from > to)
            throw new ValidationException("'from' must be earlier than or equal to 'to'.");

        var confirmed = await bookings.GetConfirmedAsync(from, to, ct);

        var rows = confirmed
            .Select((b, i) => new BookingReportRowDto(
                i + 1,
                b.Room?.RoomNumber ?? string.Empty,
                b.Room?.RoomType.ToString() ?? string.Empty,
                b.GuestName,
                b.CheckIn.ToString(DateFormat, CultureInfo.InvariantCulture),
                b.CheckOut.ToString(DateFormat, CultureInfo.InvariantCulture),
                b.Nights,
                FormatMoney(b.TotalPrice),
                b.TotalPrice))
            .ToList();

        return new BookingReportDto(from, to, rows.Count, FormatMoney(rows.Sum(r => r.TotalPriceValue)), rows);
    }

    /// <summary>1900000 -> "1.900.000"</summary>
    public static string FormatMoney(decimal amount) => amount.ToString("#,0", Vietnamese);
}
