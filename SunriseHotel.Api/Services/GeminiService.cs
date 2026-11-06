using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SunriseHotel.Api.DTOs;

namespace SunriseHotel.Api.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiService> _logger;

    private readonly string _apiKey;
    private readonly string _model;

    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        // Requirement 3: API key must NOT be hard-coded.
        // Check configuration (User Secrets / appsettings) first, then environment variable.
        _apiKey = _configuration["Gemini:ApiKey"]
                  ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                  ?? Environment.GetEnvironmentVariable("Gemini__ApiKey")
                  ?? string.Empty;

        _model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";
    }

    private void EnsureApiKeyConfigured()
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException(
                "Gemini API key chưa được cấu hình. Vui lòng thiết lập biến môi trường GEMINI_API_KEY " +
                "hoặc dùng User Secrets: 'dotnet user-secrets set Gemini:ApiKey <YOUR_KEY>'.");
        }
    }

    public async Task<string> AskHotelAsync(string question)
    {
        EnsureApiKeyConfigured();

        const string systemPrompt =
            "Bạn là lễ tân lịch sự, chu đáo và nhiệt tình tại Sunrise Hotel (địa chỉ: 25 Trần Phú, Nha Trang).\n" +
            "Thông tin khách sạn:\n" +
            "- Giờ check-in: từ 14:00.\n" +
            "- Giờ check-out: trước 12:00.\n" +
            "- Bữa sáng miễn phí: từ 6:30 đến 9:30.\n" +
            "- Đưa đón sân bay miễn phí cho hạng phòng Suite.\n" +
            "- Hồ bơi: ở tầng 5.\n" +
            "- Các loại phòng:\n" +
            "  + Single (Phòng đơn): 600.000 VND / đêm, tối đa 1 khách.\n" +
            "  + Double (Phòng đôi): 950.000 VND / đêm, tối đa 2 khách.\n" +
            "  + Suite (Phòng cao cấp): 2.200.000 VND / đêm, tối đa 4 khách.\n\n" +
            "QUY TẮC BẮT BUỘC:\n" +
            "1. Luôn giữ thái độ lễ phép, lịch sự của một lễ tân khách sạn chuyên nghiệp.\n" +
            "2. Chỉ trả lời các câu hỏi liên quan đến Sunrise Hotel và dịch vụ của khách sạn.\n" +
            "3. Lịch sự từ chối trả lời nếu câu hỏi không liên quan đến Sunrise Hotel (ví dụ hỏi về toán, code, lịch sử thế giới, bóng đá,...).";

        var payload = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = question } }
                }
            }
        };

        var responseJson = await SendGeminiRequestWithRetryAsync(payload);
        return ExtractTextResponse(responseJson);
    }

    public async Task<ParsedBookingDto> ParseBookingAsync(string text)
    {
        EnsureApiKeyConfigured();

        const string systemPrompt =
            "Bạn là trợ lý AI chuyên trích xuất thông tin đặt phòng tại Sunrise Hotel.\n" +
            "Nhiệm vụ: Phân tích văn bản yêu cầu đặt phòng của khách hàng và trích xuất thành JSON.\n" +
            "Quy tắc trích xuất:\n" +
            "- guestName: Họ tên khách hàng.\n" +
            "- phone: Số điện thoại liên hệ.\n" +
            "- roomType: Chuẩn hóa chính xác về một trong 3 loại: 'Single', 'Double', 'Suite'. (Ví dụ: 'phòng đơn' -> 'Single', 'phòng đôi' -> 'Double', 'phòng suite'/'phòng vip' -> 'Suite').\n" +
            "- checkIn: Ngày nhận phòng theo định dạng 'YYYY-MM-DD'.\n" +
            "- checkOut: Ngày trả phòng theo định dạng 'YYYY-MM-DD'.\n" +
            "- guests: Số lượng khách (số nguyên).\n" +
            "Năm tham chiếu hiện tại là năm 2026. Nếu khách nhập ngày tháng không kèm năm, hãy lấy năm 2026.\n" +
            "Chỉ trả về JSON thuần túy theo schema yêu cầu.";

        var payload = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = text } }
                }
            },
            generationConfig = new
            {
                response_mime_type = "application/json",
                response_schema = new
                {
                    type = "OBJECT",
                    properties = new
                    {
                        guestName = new { type = "STRING" },
                        phone = new { type = "STRING" },
                        roomType = new { type = "STRING" },
                        checkIn = new { type = "STRING" },
                        checkOut = new { type = "STRING" },
                        guests = new { type = "INTEGER" }
                    },
                    required = new[] { "guestName", "phone", "roomType", "checkIn", "checkOut", "guests" }
                }
            }
        };

        var responseJson = await SendGeminiRequestWithRetryAsync(payload);
        var rawText = ExtractTextResponse(responseJson);

        try
        {
            // Clean markdown code blocks if any
            var cleanJson = CleanJsonString(rawText);
            var parsed = JsonSerializer.Deserialize<ParsedBookingDto>(cleanJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (parsed == null)
            {
                throw new InvalidOperationException("Không thể giải mã dữ liệu đặt phòng từ Gemini JSON.");
            }

            return parsed;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON returned from Gemini: {RawText}", rawText);
            throw new InvalidOperationException($"Lỗi phân tích JSON từ AI: {ex.Message}. Nội dung nhận được: {rawText}");
        }
    }

    public async Task<(string Reply, List<ToolCallLog> ExecutedTools)> ChatWithAssistantAsync(
        string message,
        Func<string, JsonElement, Task<object>> toolExecutor)
    {
        EnsureApiKeyConfigured();

        const string systemPrompt =
            "Bạn là trợ lý AI thông minh hỗ trợ khách hàng tại Sunrise Hotel.\n" +
            "Bạn có quyền sử dụng các function sau để phục vụ khách:\n" +
            "- CheckAvailability(roomType, checkIn, checkOut): Kiểm tra phòng trống và giá phòng trong khoảng thời gian.\n" +
            "- GetBookingsByPhone(phone): Tra cứu danh sách các đơn đặt phòng của khách theo số điện thoại.\n" +
            "- CancelBooking(bookingId): Hủy đơn đặt phòng theo mã bookingId.\n\n" +
            "Năm tham chiếu hiện tại là năm 2026. Nếu khách hỏi ngày không kèm năm, mặc định là năm 2026.\n" +
            "Khi khách yêu cầu kiểm tra phòng, tra cứu hoặc hủy phòng, hãy gọi function thích hợp.\n" +
            "Sau khi nhận kết quả từ function, hãy trả lời khách hàng một cách lịch sự, rõ ràng và đầy đủ bằng tiếng Việt.";

        var toolDeclarations = new object[]
        {
            new
            {
                name = "CheckAvailability",
                description = "Return available rooms and price for given room type and date range",
                parameters = new
                {
                    type = "OBJECT",
                    properties = new
                    {
                        roomType = new { type = "STRING", description = "Room type: Single, Double, or Suite (optional)" },
                        checkIn = new { type = "STRING", description = "Check-in date (YYYY-MM-DD)" },
                        checkOut = new { type = "STRING", description = "Check-out date (YYYY-MM-DD)" }
                    },
                    required = new[] { "checkIn", "checkOut" }
                }
            },
            new
            {
                name = "GetBookingsByPhone",
                description = "Return the bookings of a guest by phone number",
                parameters = new
                {
                    type = "OBJECT",
                    properties = new
                    {
                        phone = new { type = "STRING", description = "Guest phone number" }
                    },
                    required = new[] { "phone" }
                }
            },
            new
            {
                name = "CancelBooking",
                description = "Change booking status to Cancelled",
                parameters = new
                {
                    type = "OBJECT",
                    properties = new
                    {
                        bookingId = new { type = "INTEGER", description = "The ID of the booking to cancel" }
                    },
                    required = new[] { "bookingId" }
                }
            }
        };

        var contentsList = new List<object>
        {
            new
            {
                role = "user",
                parts = new object[] { new { text = message } }
            }
        };

        var executedTools = new List<ToolCallLog>();
        const int maxTurns = 5;

        for (int turn = 0; turn < maxTurns; turn++)
        {
            var payload = new
            {
                system_instruction = new
                {
                    parts = new[] { new { text = systemPrompt } }
                },
                contents = contentsList,
                tools = new[]
                {
                    new { function_declarations = toolDeclarations }
                }
            };

            var responseJson = await SendGeminiRequestWithRetryAsync(payload);
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                return ("Xin lỗi, tôi không thể xử lý yêu cầu lúc này.", executedTools);
            }

            var firstCandidate = candidates[0];
            var content = firstCandidate.GetProperty("content");
            var parts = content.GetProperty("parts");

            // Check if model called a function
            bool hasFunctionCall = false;
            string? functionName = null;
            JsonElement functionArgs = default;

            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("functionCall", out var call))
                {
                    hasFunctionCall = true;
                    functionName = call.GetProperty("name").GetString();
                    functionArgs = call.GetProperty("args").Clone();
                    break;
                }
            }

            if (!hasFunctionCall || string.IsNullOrEmpty(functionName))
            {
                // No function call -> Final text answer
                var finalText = ExtractTextResponse(responseJson);
                return (finalText, executedTools);
            }

            // Execute C# Function
            _logger.LogInformation("Gemini called function: {FunctionName} with args: {Args}",
                functionName, functionArgs.GetRawText());

            var toolResult = await toolExecutor(functionName, functionArgs);
            executedTools.Add(new ToolCallLog
            {
                FunctionName = functionName,
                Arguments = JsonSerializer.Deserialize<object>(functionArgs.GetRawText()),
                Result = toolResult
            });

            // Append model response to conversation history
            contentsList.Add(new
            {
                role = "model",
                parts = new object[]
                {
                    new
                    {
                        functionCall = new
                        {
                            name = functionName,
                            args = JsonSerializer.Deserialize<object>(functionArgs.GetRawText())
                        }
                    }
                }
            });

            // Append function response to conversation history
            contentsList.Add(new
            {
                role = "function",
                parts = new object[]
                {
                    new
                    {
                        functionResponse = new
                        {
                            name = functionName,
                            response = new
                            {
                                name = functionName,
                                content = toolResult
                            }
                        }
                    }
                }
            });
        }

        return ("Đã hoàn thành các thao tác theo yêu cầu của quý khách.", executedTools);
    }

    private async Task<string> SendGeminiRequestWithRetryAsync(object payload)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
        var jsonContent = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null, // Keep snake_case and camelCase as defined
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        int maxRetries = 2;
        int delayMs = 1500;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.TooManyRequests) // 429 Quota
                {
                    _logger.LogWarning("Gemini API returned 429 Quota Exceeded (Attempt {Attempt}/{MaxRetries})",
                        attempt + 1, maxRetries + 1);

                    if (attempt < maxRetries)
                    {
                        await Task.Delay(delayMs);
                        delayMs *= 2;
                        continue;
                    }

                    throw new HttpRequestException("Gemini API Quota Exceeded (HTTP 429). Vui lòng đợi vài giây và thử lại.", null, HttpStatusCode.TooManyRequests);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API error {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
                    throw new HttpRequestException($"Lỗi gọi Gemini API (HTTP {(int)response.StatusCode}): {errorBody}", null, response.StatusCode);
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException) when (attempt < maxRetries)
            {
                await Task.Delay(delayMs);
                delayMs *= 2;
            }
        }

        throw new InvalidOperationException("Không thể kết nối đến Gemini API sau nhiều lần thử.");
    }

    private static string ExtractTextResponse(string responseJson)
    {
        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var candidate = candidates[0];
            if (candidate.TryGetProperty("content", out var content) &&
                content.TryGetProperty("parts", out var parts))
            {
                var sb = new StringBuilder();
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var text))
                    {
                        sb.Append(text.GetString());
                    }
                }
                return sb.ToString().Trim();
            }
        }

        return string.Empty;
    }

    private static string CleanJsonString(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "{}";
        var cleaned = raw.Trim();

        if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned.Substring(7);
        }
        else if (cleaned.StartsWith("```"))
        {
            cleaned = cleaned.Substring(3);
        }

        if (cleaned.EndsWith("```"))
        {
            cleaned = cleaned.Substring(0, cleaned.Length - 3);
        }

        return cleaned.Trim();
    }
}

