using System.Globalization;
using System.Text.Json;
using SunriseHotel.Application.Common.Exceptions;
using SunriseHotel.Domain.Enums;

namespace SunriseHotel.Application.Features.Assistant.Functions;

/// <summary>Helpers to read and validate arguments sent by the model.</summary>
internal static class FunctionArguments
{
    public static string GetString(JsonElement args, string name)
    {
        if (args.ValueKind == JsonValueKind.Object && args.TryGetProperty(name, out var value))
        {
            var text = value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.GetRawText(),
                _ => null
            };
            if (!string.IsNullOrWhiteSpace(text)) return text.Trim();
        }
        throw new ValidationException($"Argument '{name}' is required.");
    }

    public static int GetInt(JsonElement args, string name)
    {
        if (args.ValueKind == JsonValueKind.Object && args.TryGetProperty(name, out var value))
        {
            if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d)
                && d == Math.Floor(d) && d is >= int.MinValue and <= int.MaxValue)
                return (int)d;
            if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var i))
                return i;
        }
        throw new ValidationException($"Argument '{name}' must be an integer.");
    }

    public static DateOnly GetDate(JsonElement args, string name)
    {
        var text = GetString(args, name);
        return DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? d
            : throw new ValidationException($"Argument '{name}' must be a date in yyyy-MM-dd format.");
    }

    public static RoomType GetRoomType(JsonElement args, string name)
    {
        var text = GetString(args, name);
        return !int.TryParse(text, out _) && Enum.TryParse<RoomType>(text, ignoreCase: true, out var t) && Enum.IsDefined(t)
            ? t
            : throw new ValidationException($"Argument '{name}' must be Single, Double or Suite.");
    }
}
