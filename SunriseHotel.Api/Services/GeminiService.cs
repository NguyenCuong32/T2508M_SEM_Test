using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using SunriseHotel.Api.Data;
using SunriseHotel.Api.DTOs;

namespace SunriseHotel.Api.Services;

public class GeminiService
{
    private readonly IConfiguration _config;
    private readonly SunriseHotelDbContext _db;
    private readonly ILogger<GeminiService> _logger;
    private readonly string _apiKey;
    private readonly string _modelName;

    public const string HotelSystemInstruction = 
        "You are a polite, helpful receptionist at Sunrise Hotel (25 Tran Phu, Nha Trang). " +
        "Hotel Information: " +
        "- Address: 25 Tran Phu, Nha Trang. " +
        "- Check-in time: from 14:00. " +
        "- Check-out time: before 12:00. " +
        "- Free breakfast: 6:30 - 9:30. " +
        "- Swimming pool: 5th floor. " +
        "- Free airport pick-up for Suite rooms. " +
        "- Room types and rates: Single (600,000 VND/night), Double (950,000 VND/night), Suite (2,200,000 VND/night). " +
        "Rules: " +
        "1. Always reply in natural, polite Vietnamese. " +
        "2. Do NOT output code, python commands, or function syntax. " +
        "3. ONLY answer questions related to Sunrise Hotel. " +
        "4. If a guest asks about booking or dates, warmly provide room rates and check-in details, and guide them to provide their full booking request (name, phone, room type, dates). " +
        "5. If a question is not related to Sunrise Hotel, politely refuse to answer.";

    public GeminiService(IConfiguration config, SunriseHotelDbContext db, ILogger<GeminiService> logger)
    {
        _config = config;
        _db = db;
        _logger = logger;
        _apiKey = _config["Gemini:ApiKey"] 
                  ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
                  ?? string.Empty;
        _modelName = _config["Gemini:Model"] ?? "gemini-3.5-flash";
    }

    private GenerativeModel CreateModel()
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException("Gemini API key is not configured. Please set it via User Secrets or GEMINI_API_KEY environment variable.");
        }
        var googleAI = new GoogleAI(_apiKey);
        return googleAI.GenerativeModel(_modelName);
    }

    public async Task<string> AskHotelAsync(string question)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var model = CreateModel();
            var request = new GenerateContentRequest
            {
                Contents = new List<Content>
                {
                    new Content
                    {
                        Role = "user",
                        Parts = new List<IPart> { new TextData { Text = question } }
                    }
                },
                SystemInstruction = new Content
                {
                    Role = "system",
                    Parts = new List<IPart> { new TextData { Text = HotelSystemInstruction } }
                }
            };

            var response = await model.GenerateContent(request);
            var text = response?.Text;

            // Robust text extraction fallback
            if (string.IsNullOrWhiteSpace(text) && response?.Candidates?.Count > 0)
            {
                var candidate = response.Candidates[0];
                if (candidate.Content?.Parts != null)
                {
                    var textParts = candidate.Content.Parts
                        .Select(p => p.Text)
                        .Where(t => !string.IsNullOrWhiteSpace(t));
                    text = string.Join("\n", textParts);
                }
            }

            return text ?? "Dạ Sunrise Hotel xin chào quý khách! Tôi có thể hỗ trợ gì cho quý khách ạ?";
        });
    }

    public async Task<ParsedBookingData> ParseBookingAsync(string text)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var model = CreateModel();

            var prompt = 
                "Extract booking details from the following guest request into a clean JSON object.\n" +
                "Rules:\n" +
                "- guestName: Full name of the guest.\n" +
                "- phone: Phone number.\n" +
                "- roomType: Standardize to one of exactly: 'Single', 'Double', or 'Suite' ('phòng đơn' -> 'Single', 'phòng đôi' -> 'Double', 'phòng suite' -> 'Suite').\n" +
                "- checkIn: Check-in date formatted as YYYY-MM-DD.\n" +
                "- checkOut: Check-out date formatted as YYYY-MM-DD.\n" +
                "- guests: Number of guests as an integer.\n\n" +
                "Expected JSON format:\n" +
                "{\n" +
                "  \"guestName\": \"string\",\n" +
                "  \"phone\": \"string\",\n" +
                "  \"roomType\": \"Single | Double | Suite\",\n" +
                "  \"checkIn\": \"YYYY-MM-DD\",\n" +
                "  \"checkOut\": \"YYYY-MM-DD\",\n" +
                "  \"guests\": 2\n" +
                "}\n\n" +
                $"Input: \"{text}\"";

            var request = new GenerateContentRequest
            {
                Contents = new List<Content>
                {
                    new Content
                    {
                        Role = "user",
                        Parts = new List<IPart> { new TextData { Text = prompt } }
                    }
                },
                GenerationConfig = new GenerationConfig
                {
                    ResponseMimeType = "application/json"
                }
            };

            var response = await model.GenerateContent(request);
            var rawJson = response?.Text ?? string.Empty;

            rawJson = CleanJsonString(rawJson);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var parsed = JsonSerializer.Deserialize<ParsedBookingData>(rawJson, options);
            if (parsed == null)
            {
                throw new FormatException("Failed to deserialize Gemini output into booking data: " + rawJson);
            }

            return parsed;
        });
    }

    /// <summary>
    /// Requirement 3: bookingassistant with Function Calling
    /// Declares CheckAvailability, GetBookingsByPhone, CancelBooking
    /// </summary>
    public async Task<string> ChatAssistantAsync(string userMessage)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var model = CreateModel();

            var tools = new Tools();
            tools.AddFunction("CheckAvailability", "Return available rooms and price. Parameters: roomType (Single, Double, or Suite), checkIn (YYYY-MM-DD), checkOut (YYYY-MM-DD).", 
                (string roomType, string checkIn, string checkOut) => CheckAvailability(roomType, checkIn, checkOut));

            tools.AddFunction("GetBookingsByPhone", "Return the bookings of a guest. Parameters: phone (guest phone number).", 
                (string phone) => GetBookingsByPhone(phone));

            tools.AddFunction("CancelBooking", "Change booking status to Cancelled. Parameters: bookingId (integer ID of booking to cancel).", 
                (int bookingId) => CancelBooking(bookingId));

            var chat = model.StartChat(
                tools: tools,
                enableAutomaticFunctionCalling: true
            );

            var response = await chat.SendMessage(userMessage);
            return response?.Text ?? "Xin lỗi, tôi không thể xử lý yêu cầu lúc này.";
        });
    }

    // --- Retry & Bonus 429 Error Handling Helper ---
    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action, int maxRetries = 2)
    {
        for (int attempt = 1; attempt <= maxRetries + 1; attempt++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                var isQuota429 = ex.Message.Contains("429") || ex.Message.Contains("quota", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("Too Many Requests", StringComparison.OrdinalIgnoreCase);

                if (isQuota429 && attempt <= maxRetries)
                {
                    _logger.LogWarning("Gemini API 429 Quota Exceeded. Retrying in 2 seconds... (Attempt {Attempt}/{MaxRetries})", attempt, maxRetries);
                    await Task.Delay(2000);
                    continue;
                }

                if (isQuota429)
                {
                    throw new InvalidOperationException("Hệ thống AI đang tạm thời vượt quá hạn ngạch (Quota 429). Quý khách vui lòng chờ 5-10 giây rồi thử lại ạ!");
                }

                throw;
            }
        }

        throw new InvalidOperationException("Không thể hoàn tất kết nối tới dịch vụ AI.");
    }

    // --- C# Tool Implementations ---

    public string CheckAvailability(string roomType, string checkIn, string checkOut)
    {
        if (!DateOnly.TryParse(checkIn, out var inDate) || !DateOnly.TryParse(checkOut, out var outDate))
        {
            return JsonSerializer.Serialize(new { error = "Invalid date format. Expected YYYY-MM-DD." });
        }

        var bookedRoomIds = _db.Bookings
            .Where(b => b.Status == "Confirmed" && b.CheckIn < outDate && b.CheckOut > inDate)
            .Select(b => b.RoomId)
            .ToList();

        var availableRooms = _db.Rooms
            .Where(r => r.RoomType.ToLower() == roomType.ToLower() && !bookedRoomIds.Contains(r.RoomId))
            .Select(r => new { r.RoomNumber, r.RoomType, r.PricePerNight, r.MaxGuests })
            .ToList();

        if (availableRooms.Count == 0)
        {
            return JsonSerializer.Serialize(new { message = $"No available {roomType} rooms between {checkIn} and {checkOut}." });
        }

        return JsonSerializer.Serialize(new
        {
            message = $"Found {availableRooms.Count} available {roomType} room(s).",
            availableRooms
        });
    }

    public string GetBookingsByPhone(string phone)
    {
        var bookings = _db.Bookings
            .Include(b => b.Room)
            .Where(b => b.Phone == phone)
            .OrderByDescending(b => b.BookingId)
            .Select(b => new
            {
                b.BookingId,
                b.GuestName,
                b.Phone,
                RoomNumber = b.Room != null ? b.Room.RoomNumber : "N/A",
                RoomType = b.Room != null ? b.Room.RoomType : "N/A",
                CheckIn = b.CheckIn.ToString("yyyy-MM-dd"),
                CheckOut = b.CheckOut.ToString("yyyy-MM-dd"),
                b.TotalPrice,
                b.Status
            })
            .ToList();

        if (bookings.Count == 0)
        {
            return JsonSerializer.Serialize(new { message = $"No bookings found for phone number {phone}." });
        }

        return JsonSerializer.Serialize(bookings);
    }

    public string CancelBooking(int bookingId)
    {
        var booking = _db.Bookings.Find(bookingId);
        if (booking == null)
        {
            return JsonSerializer.Serialize(new { error = $"Booking ID {bookingId} not found." });
        }

        if (booking.Status == "Cancelled")
        {
            return JsonSerializer.Serialize(new { message = $"Booking ID {bookingId} is already Cancelled." });
        }

        booking.Status = "Cancelled";
        _db.SaveChanges();

        return JsonSerializer.Serialize(new { message = $"Booking ID {bookingId} has been successfully changed to Cancelled." });
    }

    private static string CleanJsonString(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "{}";
        var cleaned = text.Trim();
        if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned.Substring(7);
        }
        else if (cleaned.StartsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned.Substring(3);
        }

        if (cleaned.EndsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned.Substring(0, cleaned.Length - 3);
        }

        return cleaned.Trim();
    }
}
