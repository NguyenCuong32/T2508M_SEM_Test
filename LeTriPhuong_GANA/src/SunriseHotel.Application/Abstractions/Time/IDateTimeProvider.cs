namespace SunriseHotel.Application.Abstractions.Time;

public interface IDateTimeProvider
{
    /// <summary>Current date in the hotel's time zone (Asia/Ho_Chi_Minh).</summary>
    DateOnly Today { get; }
}
