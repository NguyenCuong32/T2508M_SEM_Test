using SunriseHotel.Models;
using SunriseHotel.Repositories;
namespace SunriseHotel.Services;

public class BookingService(IGeminiService ai, IBookingRepository repository) : IBookingService
{
    public async Task<BookingResult> CreateFromText(string text)
    {
        var parsed = await ai.Parse(text);
        if (string.IsNullOrWhiteSpace(parsed.GuestName) || parsed.GuestName.Length > 128 ||
            string.IsNullOrWhiteSpace(parsed.Phone) || parsed.Phone.Length > 20 ||
            !GeminiService.ValidType(parsed.RoomType) || parsed.CheckIn == default ||
            parsed.CheckOut <= parsed.CheckIn || parsed.Guests < 1)
            return new(parsed, null, "Thông tin đặt phòng không hợp lệ hoặc chưa đầy đủ.");
        return new(parsed, await repository.Create(parsed), null);
    }
}
