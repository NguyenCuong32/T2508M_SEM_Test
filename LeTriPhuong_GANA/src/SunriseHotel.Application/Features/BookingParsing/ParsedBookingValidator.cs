using System.Globalization;
using SunriseHotel.Application.Abstractions.Time;
using SunriseHotel.Application.Features.Bookings;
using SunriseHotel.Domain.Enums;

namespace SunriseHotel.Application.Features.BookingParsing;

public interface IParsedBookingValidator
{
    /// <summary>Validates the AI output and converts it to a strongly-typed command.
    /// Throws <see cref="Common.Exceptions.ValidationException"/> listing every problem.</summary>
    CreateBookingCommand Validate(ParsedBookingDto parsed);
}

public sealed class ParsedBookingValidator(IDateTimeProvider clock) : IParsedBookingValidator
{
    private const int MaxGuests = 4;

    public CreateBookingCommand Validate(ParsedBookingDto parsed)
    {
        var errors = new List<string>();

        var guestName = parsed.GuestName?.Trim() ?? string.Empty;
        if (guestName.Length == 0) errors.Add("guestName is missing.");
        else if (guestName.Length > 128) errors.Add("guestName must not exceed 128 characters.");

        var phone = BookingRules.NormalizePhone(parsed.Phone);
        if (phone.Length == 0) errors.Add("phone is missing.");
        else if (!BookingRules.IsValidPhone(phone)) errors.Add($"phone '{parsed.Phone}' is invalid.");

        var roomTypeValid = Enum.TryParse<RoomType>(parsed.RoomType, ignoreCase: true, out var roomType)
                            && Enum.IsDefined(roomType)
                            && !int.TryParse(parsed.RoomType, out _);
        if (!roomTypeValid)
            errors.Add($"roomType '{parsed.RoomType}' is invalid. Allowed values: Single, Double, Suite.");

        var checkInValid = TryParseDate(parsed.CheckIn, out var checkIn);
        if (!checkInValid) errors.Add($"checkIn '{parsed.CheckIn}' is not a valid date (yyyy-MM-dd).");

        var checkOutValid = TryParseDate(parsed.CheckOut, out var checkOut);
        if (!checkOutValid) errors.Add($"checkOut '{parsed.CheckOut}' is not a valid date (yyyy-MM-dd).");

        if (checkInValid && checkOutValid)
            BookingRules.EnsureValidStay(checkIn, checkOut, clock.Today, errors);

        var guests = parsed.Guests ?? 1;
        if (guests is < 1 or > MaxGuests) errors.Add($"guests must be between 1 and {MaxGuests}.");

        BookingRules.ThrowIfAny(errors, details: new { parsed });

        return new CreateBookingCommand(guestName, phone, roomType, checkIn, checkOut, guests);
    }

    private static bool TryParseDate(string? value, out DateOnly date) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
}
