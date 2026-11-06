using System.Text.RegularExpressions;
using SunriseHotel.Application.Common.Exceptions;

namespace SunriseHotel.Application.Features.Bookings;

/// <summary>Reusable input rules for booking data.</summary>
public static partial class BookingRules
{
    public const int MaxNights = 30;

    [GeneratedRegex(@"^0\d{9,10}$")]
    private static partial Regex PhoneRegex();

    /// <summary>Removes spaces, dots, dashes and converts +84 / 84 prefix to 0.</summary>
    public static string NormalizePhone(string? phone)
    {
        var digits = new string((phone ?? string.Empty).Where(c => char.IsDigit(c) || c == '+').ToArray());
        if (digits.StartsWith("+84")) digits = "0" + digits[3..];
        else if (digits.StartsWith("84") && digits.Length == 11) digits = "0" + digits[2..];
        return digits.Replace("+", string.Empty);
    }

    public static bool IsValidPhone(string phone) => PhoneRegex().IsMatch(phone);

    public static void EnsureValidStay(DateOnly checkIn, DateOnly checkOut, DateOnly today, List<string> errors)
    {
        if (checkOut <= checkIn)
            errors.Add("checkOut must be after checkIn.");
        if (checkIn < today)
            errors.Add($"checkIn must not be in the past (today is {today:yyyy-MM-dd}).");
        if (checkOut.DayNumber - checkIn.DayNumber > MaxNights)
            errors.Add($"A booking cannot exceed {MaxNights} nights.");
    }

    public static void ThrowIfAny(List<string> errors, object? details = null)
    {
        if (errors.Count > 0) throw new ValidationException(errors, details);
    }
}
