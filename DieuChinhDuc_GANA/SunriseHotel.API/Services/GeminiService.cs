using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SunriseHotel.API.DTOs;

namespace SunriseHotel.API.Services;

public sealed class GeminiService(HttpClient client, IConfiguration configuration, BookingService bookings)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonElement BookingSchema = JsonSerializer.Deserialize<JsonElement>("""
        {
          "type": "object",
          "properties": {
            "guestName": { "type": ["string", "null"] },
            "phone": { "type": ["string", "null"] },
            "roomType": { "type": ["string", "null"], "enum": ["Single", "Double", "Suite", null] },
            "checkIn": { "type": ["string", "null"], "format": "date" },
            "checkOut": { "type": ["string", "null"], "format": "date" },
            "guests": { "type": ["integer", "null"] }
          },
          "required": ["guestName", "phone", "roomType", "checkIn", "checkOut", "guests"]
        }
        """);

    private static readonly JsonElement AssistantTools = JsonSerializer.Deserialize<JsonElement>("""
        [{
          "functionDeclarations": [
            {
              "name": "CheckAvailability",
              "description": "Find available rooms of a given type and their nightly price in VND.",
              "parameters": {
                "type": "OBJECT",
                "properties": {
                  "roomType": { "type": "STRING", "enum": ["Single", "Double", "Suite"] },
                  "checkIn": { "type": "STRING", "description": "Check-in date in yyyy-MM-dd." },
                  "checkOut": { "type": "STRING", "description": "Check-out date in yyyy-MM-dd." }
                },
                "required": ["roomType", "checkIn", "checkOut"]
              }
            },
            {
              "name": "GetBookingsByPhone",
              "description": "Look up a guest's bookings by phone number.",
              "parameters": {
                "type": "OBJECT",
                "properties": {
                  "phone": { "type": "STRING", "description": "Guest phone number as provided." }
                },
                "required": ["phone"]
              }
            },
            {
              "name": "CancelBooking",
              "description": "Cancel a booking only after the guest explicitly requests cancellation and provides its ID.",
              "parameters": {
                "type": "OBJECT",
                "properties": {
                  "bookingId": { "type": "INTEGER", "description": "Positive booking ID provided by the guest." }
                },
                "required": ["bookingId"]
              }
            }
          ]
        }]
        """);

    public async Task<string> AskHotelAsync(string question, CancellationToken cancellationToken)
    {
        var content = await GenerateAsync(
            GeminiPrompts.Receptionist + "\n" + GeminiPrompts.HotelFacts,
            [UserContent(question)], cancellationToken);
        return ReadText(content);
    }

    public async Task<ParsedBooking> ParseBookingAsync(string text, CancellationToken cancellationToken)
    {
        var content = await GenerateAsync(WithLocalDate(GeminiPrompts.ParseBooking),
            [UserContent(text)], cancellationToken,
            generationConfig: new { responseMimeType = "application/json", responseJsonSchema = BookingSchema });

        try
        {
            return JsonSerializer.Deserialize<ParsedBooking>(ReadText(content), JsonOptions)
                ?? throw new JsonException();
        }
        catch (JsonException)
        {
            throw new RequestException(502, "Gemini trả về JSON đặt phòng không hợp lệ.");
        }
    }

    public async Task<string> ChatAsync(string message, CancellationToken cancellationToken)
    {
        var contents = new List<object> { UserContent(message) };
        var instruction = WithLocalDate(GeminiPrompts.Assistant + "\n" + GeminiPrompts.HotelFacts);
        for (var turn = 0; turn < 6; turn++)
        {
            var content = await GenerateAsync(instruction, contents, cancellationToken, tools: AssistantTools);
            var responses = new List<object>();
            foreach (var part in content.GetProperty("parts").EnumerateArray())
            {
                if (!part.TryGetProperty("functionCall", out var call))
                    continue;

                var name = GetString(call, "name") ?? "";
                call.TryGetProperty("args", out var args);
                var result = await ExecuteFunctionAsync(name, args, message, cancellationToken);
                responses.Add(new { functionResponse = new { name, id = GetString(call, "id"), response = result } });
            }

            if (responses.Count == 0)
                return ReadText(content);

            contents.Add(content);
            contents.Add(new { role = "user", parts = responses });
        }

        throw new RequestException(502, "Trợ lý đã vượt giới hạn lượt gọi hàm.");
    }

    private async Task<object> ExecuteFunctionAsync(
        string name, JsonElement args, string message, CancellationToken cancellationToken)
    {
        try
        {
            return name switch
            {
                "CheckAvailability" => new
                {
                    rooms = await bookings.CheckAvailabilityAsync(GetString(args, "roomType"),
                        GetString(args, "checkIn"), GetString(args, "checkOut"), cancellationToken)
                },
                "GetBookingsByPhone" => new
                {
                    bookings = await bookings.GetByPhoneAsync(GetString(args, "phone"), cancellationToken)
                },
                "CancelBooking" => await CancelBookingAsync(args, message, cancellationToken),
                _ => new { error = "Hàm không được hỗ trợ." }
            };
        }
        catch (RequestException error)
        {
            return new { error = error.Message };
        }
    }

    private async Task<object> CancelBookingAsync(JsonElement args, string message, CancellationToken cancellationToken)
    {
        if (args.ValueKind != JsonValueKind.Object || !args.TryGetProperty("bookingId", out var id)
            || id.ValueKind != JsonValueKind.Number || !id.TryGetInt32(out var bookingId) || bookingId <= 0
            || !Regex.IsMatch(message, $@"(?<![0-9]){bookingId}(?![0-9])"))
            return new { error = "Khách cần cung cấp mã đặt phòng cụ thể trong yêu cầu hủy." };

        return new { bookingId, status = await bookings.CancelAsync(bookingId, cancellationToken) };
    }

    private async Task<JsonElement> GenerateAsync(
        string instruction, IReadOnlyList<object> contents, CancellationToken cancellationToken,
        object? generationConfig = null, object? tools = null)
    {
        var key = configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(key))
            throw new RequestException(503, "Chưa cấu hình Gemini API key.");

        var model = configuration["Gemini:Model"] ?? "gemini-3.5-flash";
        using var request = new HttpRequestMessage(HttpMethod.Post, $"models/{Uri.EscapeDataString(model)}:generateContent");
        request.Headers.Add("x-goog-api-key", key);
        request.Content = JsonContent.Create(new
        {
            systemInstruction = new { parts = new[] { new { text = instruction } } },
            contents,
            generationConfig,
            tools
        }, options: JsonOptions);

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            if ((int)response.StatusCode == 429)
                throw new RequestException(429, "Gemini đã chạm giới hạn yêu cầu. Hãy thử lại sau.");

            if (!response.IsSuccessStatusCode)
                throw new RequestException(502, $"Gemini từ chối yêu cầu (HTTP {(int)response.StatusCode}).");

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("candidates", out var candidates)
                || candidates.ValueKind != JsonValueKind.Array || candidates.GetArrayLength() == 0
                || candidates[0].ValueKind != JsonValueKind.Object
                || !candidates[0].TryGetProperty("content", out var content)
                || content.ValueKind != JsonValueKind.Object
                || !content.TryGetProperty("parts", out var parts) || parts.ValueKind != JsonValueKind.Array
                || parts.EnumerateArray().Any(part => part.ValueKind != JsonValueKind.Object))
                throw new RequestException(502, "Gemini không trả về nội dung hợp lệ.");

            return content.Clone();
        }
        catch (HttpRequestException)
        {
            throw new RequestException(502, "Không thể kết nối với Gemini.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new RequestException(504, "Gemini phản hồi quá thời gian chờ.");
        }
        catch (JsonException)
        {
            throw new RequestException(502, "Phản hồi từ Gemini không phải JSON hợp lệ.");
        }
    }

    private static string ReadText(JsonElement content)
    {
        var text = string.Concat(content.GetProperty("parts").EnumerateArray()
            .Where(part => !part.TryGetProperty("thought", out var thought) || thought.ValueKind != JsonValueKind.True)
            .Select(part => GetString(part, "text"))).Trim();

        return string.IsNullOrEmpty(text)
            ? throw new RequestException(502, "Gemini không trả về câu trả lời.")
            : text;
    }

    private static object UserContent(string text) => new { role = "user", parts = new[] { new { text } } };

    private static string? GetString(JsonElement element, string property) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(property, out var value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static string WithLocalDate(string prompt)
    {
        var today = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
        return prompt + "\nNgày hiện tại tại Việt Nam: " + today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
