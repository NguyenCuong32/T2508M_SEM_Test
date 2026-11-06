using System.Globalization;
using System.Text.RegularExpressions;
using SunriseHotel.API.Data;
using SunriseHotel.API.DTOs;
using SunriseHotel.API.Models;

namespace SunriseHotel.API.Services;

public sealed partial class BookingService(BookingRepository repository)
{
    public async Task<BookingResult> CreateAsync(ParsedBooking parsed, CancellationToken cancellationToken)
    {
        var guestName = parsed.GuestName?.Trim();
        if (string.IsNullOrWhiteSpace(guestName) || guestName.Length > 128)
            throw new RequestException(400, "Họ tên khách phải có từ 1 đến 128 ký tự.");

        var phone = ValidatePhone(parsed.Phone);
        var roomType = ValidateRoomType(parsed.RoomType);
        var (checkIn, checkOut) = ValidateStay(parsed.CheckIn, parsed.CheckOut);
        if (parsed.Guests is not (> 0 and <= 4))
            throw new RequestException(400, "Số khách phải từ 1 đến 4.");

        return await repository.CreateAsync(
            guestName, phone, roomType, checkIn, checkOut, parsed.Guests.Value, cancellationToken)
            ?? throw new RequestException(409, "Không còn phòng phù hợp với số khách và ngày lưu trú.");
    }

    public Task<IReadOnlyList<Room>> CheckAvailabilityAsync(
        string? roomType, string? checkIn, string? checkOut, CancellationToken cancellationToken)
    {
        var (start, end) = ValidateStay(ParseDate(checkIn), ParseDate(checkOut));
        return repository.CheckAvailabilityAsync(ValidateRoomType(roomType), start, end, cancellationToken);
    }

    public Task<IReadOnlyList<Booking>> GetByPhoneAsync(string? phone, CancellationToken cancellationToken) =>
        repository.GetByPhoneAsync(ValidatePhone(phone), cancellationToken);

    public async Task<string> CancelAsync(int bookingId, CancellationToken cancellationToken)
    {
        if (bookingId <= 0)
            throw new RequestException(400, "Mã đặt phòng phải là số nguyên dương.");

        return await repository.CancelAsync(bookingId, cancellationToken)
            ?? throw new RequestException(404, "Không tìm thấy đơn đặt phòng.");
    }

    public Task<IReadOnlyList<BookingReportRow>> GetReportAsync(
        DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        if (from > to)
            throw new RequestException(400, "Ngày bắt đầu phải trước hoặc bằng ngày kết thúc.");

        return repository.GetReportAsync(from, to, cancellationToken);
    }

    private static (DateOnly CheckIn, DateOnly CheckOut) ValidateStay(DateOnly? checkIn, DateOnly? checkOut)
    {
        if (checkIn is null || checkOut is null || checkIn.Value.Year < 1000 || checkOut <= checkIn)
            throw new RequestException(400, "Ngày trả phòng phải sau ngày nhận phòng và cả hai ngày phải hợp lệ.");

        return (checkIn.Value, checkOut.Value);
    }

    private static string ValidateRoomType(string? roomType) => roomType?.Trim().ToLowerInvariant() switch
    {
        "single" => "Single",
        "double" => "Double",
        "suite" => "Suite",
        _ => throw new RequestException(400, "Loại phòng phải là Single, Double hoặc Suite.")
    };

    private static string ValidatePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new RequestException(400, "Cần nhập số điện thoại của khách.");

        var normalized = phone.Replace(" ", "").Replace("-", "").Replace(".", "").Trim();
        if (!PhonePattern().IsMatch(normalized))
            throw new RequestException(400, "Số điện thoại phải có từ 8 đến 15 chữ số.");

        return normalized;
    }

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;

    [GeneratedRegex(@"\A\+?[0-9]{8,15}\z")]
    private static partial Regex PhonePattern();
}
