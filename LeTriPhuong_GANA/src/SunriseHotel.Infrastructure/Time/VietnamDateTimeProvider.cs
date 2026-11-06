using SunriseHotel.Application.Abstractions.Time;

namespace SunriseHotel.Infrastructure.Time;

/// <summary>Returns "today" in the hotel's time zone (UTC+7), independent of the server time zone.</summary>
public sealed class VietnamDateTimeProvider(TimeProvider timeProvider) : IDateTimeProvider
{
    private static readonly TimeZoneInfo HotelTimeZone = ResolveTimeZone();

    public DateOnly Today =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), HotelTimeZone).DateTime);

    private static TimeZoneInfo ResolveTimeZone()
    {
        foreach (var id in new[] { "Asia/Ho_Chi_Minh", "SE Asia Standard Time" })
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var tz)) return tz;
        }
        return TimeZoneInfo.CreateCustomTimeZone("UTC+7", TimeSpan.FromHours(7), "UTC+7", "UTC+7");
    }
}
