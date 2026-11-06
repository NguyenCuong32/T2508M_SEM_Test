using SUNRISEHOTEL.DTOs;

namespace SUNRISEHOTEL.Services
{
    public interface IGeminiService
    {
        Task<string> AskAsync(
            string prompt,
            string systemInstruction);

        Task<BookingRequestDTO> ParseBookingAsync(string text);
    }
}
