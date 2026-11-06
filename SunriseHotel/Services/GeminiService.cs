using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SunriseHotel.Models;
using SunriseHotel.Models.DTOs;

namespace SunriseHotel.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private string? _runtimeApiKey;
    private readonly string[] _modelCandidates = new[]
    {
        "gemini-3.5-flash-lite",
        "gemini-3.5-flash",
        "gemini-flash-latest",
        "gemini-3.8-flash"
    };

    public GeminiService(HttpClient httpClient, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(25);
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    public string GetApiKeyStatus()
    {
        var key = ResolveApiKey();
        if (string.IsNullOrWhiteSpace(key))
        {
            return "Chưa cấu hình API Key";
        }
        if (key.Length <= 8)
        {
            return "Đã cấu hình (" + key + ")";
        }
        return "Đã cấu hình (" + key.Substring(0, 4) + "..." + key.Substring(key.Length - 4) + ")";
    }

    public void SetApiKey(string apiKey)
    {
        _runtimeApiKey = CleanKey(apiKey);
        Environment.SetEnvironmentVariable("GEMINI_API_KEY", _runtimeApiKey);

        var envTargets = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"),
            Path.Combine(AppContext.BaseDirectory, ".env")
        };

        foreach (var target in envTargets)
        {
            try
            {
                var dir = Path.GetDirectoryName(target);
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                {
                    File.WriteAllText(target, "GEMINI_API_KEY=" + _runtimeApiKey + Environment.NewLine);
                }
            }
            catch
            {
            }
        }
    }

    private static string CleanKey(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        return raw.Trim().Trim('\"', '\'').Trim();
    }

    private string ResolveApiKey()
    {
        if (!string.IsNullOrWhiteSpace(_runtimeApiKey))
        {
            return _runtimeApiKey;
        }

        var envKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (!string.IsNullOrWhiteSpace(envKey))
        {
            _runtimeApiKey = CleanKey(envKey);
            return _runtimeApiKey;
        }

        var envLocations = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"),
            Path.Combine(AppContext.BaseDirectory, ".env")
        };

        foreach (var envPath in envLocations)
        {
            try
            {
                if (File.Exists(envPath))
                {
                    var lines = File.ReadAllLines(envPath);
                    foreach (var line in lines)
                    {
                        var trimmed = line.Trim();
                        if (trimmed.StartsWith("GEMINI_API_KEY="))
                        {
                            var val = trimmed.Substring("GEMINI_API_KEY=".Length);
                            var cleaned = CleanKey(val);
                            if (!string.IsNullOrWhiteSpace(cleaned))
                            {
                                _runtimeApiKey = cleaned;
                                Environment.SetEnvironmentVariable("GEMINI_API_KEY", _runtimeApiKey);
                                return _runtimeApiKey;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        var configKey = _configuration["Gemini:ApiKey"];
        if (!string.IsNullOrWhiteSpace(configKey) && !configKey.Contains("YOUR_GEMINI_API_KEY"))
        {
            _runtimeApiKey = CleanKey(configKey);
            return _runtimeApiKey;
        }

        var googleKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
        if (!string.IsNullOrWhiteSpace(googleKey))
        {
            _runtimeApiKey = CleanKey(googleKey);
            return _runtimeApiKey;
        }

        return string.Empty;
    }

    public async Task<string> AskHotelAsync(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("Câu hỏi không được để trống.");
        }

        var apiKey = ResolveApiKey();
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            try
            {
                var systemPrompt = @"Bạn là nhân viên lễ tân lịch sự, chu đáo và chuyên nghiệp của khách sạn Sunrise Hotel (địa chỉ: 25 Trần Phú, Hà Nội).
Thông tin chi tiết về khách sạn Sunrise Hotel:
- Địa chỉ: 25 Trần Phú, Hà Nội.
- Giờ nhận phòng (check-in): từ 14:00.
- Giờ trả phòng (check-out): trước 12:00.
- Bữa sáng: miễn phí từ 6:30 đến 9:30 hàng ngày.
- Tiện ích: Hồ bơi tại tầng 5.
- Dịch vụ đón tiễn sân bay: miễn phí đón tiễn sân bay Nội Bài dành riêng cho khách đặt phòng hạng Suite.
- Danh sách loại phòng và giá niêm yết:
  + Phòng Single (Đơn): 600.000 VND / đêm, sức chứa tối đa 1 khách (Phòng 101, 102).
  + Phòng Double (Đôi): 950.000 VND / đêm, sức chứa tối đa 2 khách (Phòng 201, 202).
  + Phòng Suite (Cao cấp): 2.200.000 VND / đêm, sức chứa tối đa 4 khách (Phòng 301, miễn phí đón sân bay).

QUY TẮC BẮT BUỘC:
- Chỉ trả lời các câu hỏi và thắc mắc liên quan trực tiếp đến thông tin và dịch vụ của Sunrise Hotel.
- Nếu khách hỏi về bất kỳ chủ đề nào không liên quan đến Sunrise Hotel (thời tiết bên ngoài, chính trị, địa điểm khác, kiến thức chung...), bạn phải lịch sự từ chối và giải thích rằng bạn là lễ tân của Sunrise Hotel nên chỉ hỗ trợ các thông tin về khách sạn.
- Sử dụng tiếng Việt chuẩn mực, lịch sự, hiếu khách, xưng hô 'Quý khách' và 'em' hoặc 'Sunrise Hotel'.";

                var payload = new JsonObject
                {
                    ["system_instruction"] = new JsonObject
                    {
                        ["parts"] = new JsonArray { new JsonObject { ["text"] = systemPrompt } }
                    },
                    ["contents"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["role"] = "user",
                            ["parts"] = new JsonArray { new JsonObject { ["text"] = question } }
                        }
                    }
                };

                var responseJson = await SendGeminiRequestWithRetryAsync(payload, apiKey);
                var answer = ExtractTextFromGeminiResponse(responseJson);
                if (!string.IsNullOrWhiteSpace(answer))
                {
                    return answer;
                }
            }
            catch
            {
            }
        }

        return FallbackAskHotelResponse(question);
    }

    public async Task<ParsedBookingDto?> ParseBookingAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Nội dung yêu cầu đặt phòng không được để trống.");
        }

        var apiKey = ResolveApiKey();
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            try
            {
                var systemPrompt = @"Bạn là trợ lý trích xuất dữ liệu đặt phòng của Sunrise Hotel. Nhiệm vụ của bạn là trích xuất thông tin đặt phòng từ câu nói tự nhiên của khách thành đúng định dạng JSON.
Định dạng JSON yêu cầu:
{
  ""guestName"": ""Họ và tên khách"",
  ""phone"": ""Số điện thoại"",
  ""roomType"": ""Single"" hoặc ""Double"" hoặc ""Suite"",
  ""checkIn"": ""YYYY-MM-DD"",
  ""checkOut"": ""YYYY-MM-DD"",
  ""guests"": số lượng khách (số nguyên)
}
Quy tắc:
- roomType chỉ được nhận một trong ba giá trị: 'Single', 'Double', 'Suite'.
- Nếu khách nói phòng đơn, map thành 'Single'. Phòng đôi, map thành 'Double'. Phòng VIP, gia đình, suite, map thành 'Suite'.
- checkIn và checkOut phải chuẩn YYYY-MM-DD. Nếu không nhắc đến năm, lấy năm 2026.
- guests là số nguyên dương tương ứng số người ở.";

                var payload = new JsonObject
                {
                    ["system_instruction"] = new JsonObject
                    {
                        ["parts"] = new JsonArray { new JsonObject { ["text"] = systemPrompt } }
                    },
                    ["contents"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["role"] = "user",
                            ["parts"] = new JsonArray { new JsonObject { ["text"] = text } }
                        }
                    },
                    ["generationConfig"] = new JsonObject
                    {
                        ["responseMimeType"] = "application/json",
                        ["responseSchema"] = new JsonObject
                        {
                            ["type"] = "OBJECT",
                            ["properties"] = new JsonObject
                            {
                                ["guestName"] = new JsonObject { ["type"] = "STRING" },
                                ["phone"] = new JsonObject { ["type"] = "STRING" },
                                ["roomType"] = new JsonObject
                                {
                                    ["type"] = "STRING",
                                    ["enum"] = new JsonArray { "Single", "Double", "Suite" }
                                },
                                ["checkIn"] = new JsonObject { ["type"] = "STRING" },
                                ["checkOut"] = new JsonObject { ["type"] = "STRING" },
                                ["guests"] = new JsonObject { ["type"] = "INTEGER" }
                            },
                            ["required"] = new JsonArray { "guestName", "phone", "roomType", "checkIn", "checkOut", "guests" }
                        }
                    }
                };

                var responseJson = await SendGeminiRequestWithRetryAsync(payload, apiKey);
                var rawText = ExtractTextFromGeminiResponse(responseJson);

                if (!string.IsNullOrWhiteSpace(rawText))
                {
                    var cleanedJson = CleanJsonString(rawText);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var parsed = JsonSerializer.Deserialize<ParsedBookingDto>(cleanedJson, options);
                    if (parsed != null && !string.IsNullOrWhiteSpace(parsed.GuestName))
                    {
                        return parsed;
                    }
                }
            }
            catch
            {
            }
        }

        return FallbackParseBooking(text);
    }

    public async Task<AssistantChatResponse> ChatWithAssistantAsync(string message, List<ChatHistoryItem>? history = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Nội dung tin nhắn không được để trống.");
        }

        using var scope = _serviceProvider.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var roomService = scope.ServiceProvider.GetRequiredService<IRoomService>();

        var allRooms = await roomService.GetAllRoomsAsync();
        var roomsText = string.Join("\n", allRooms.Select(r =>
            $"- Phòng {r.RoomNumber}: Hạng {r.RoomType}, Giá niêm yết: {r.PricePerNight:N0} VND/đêm, Sức chứa: {r.MaxGuests} người" +
            (r.RoomType == "Suite" ? " (Có xe đưa đón sân bay Nội Bài miễn phí, đây là phòng cao cấp nhất và đắt nhất khách sạn)" : "")));

        var todayStr = DateTime.Now.ToString("dd/MM/yyyy");
        var todayIso = DateTime.Now.ToString("yyyy-MM-dd");
        var tomorrowIso = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

        var apiKey = ResolveApiKey();
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            try
            {
                var systemPrompt = $@"Bạn là trợ lý ảo AI thông minh, chu đáo và lịch sự của khách sạn Sunrise Hotel (Địa chỉ: 25 Trần Phú, Hà Nội).
Thời gian hệ thống hiện tại: Hôm nay là ngày {todayStr} (Định dạng chuẩn YYYY-MM-DD: {todayIso}).

DANH SÁCH TOÀN BỘ PHÒNG VÀ GIÁ ĐƯỢC ĐỌC TRỰC TIẾP TỪ CƠ SỞ DỮ LIỆU MYSQL (BẢNG ROOM):
{roomsText}

THÔNG TIN QUAN TRỌNG VỀ GIÁ & QUY ĐỊNH KHÁCH SẠN:
- Phòng rẻ nhất: Hạng Single (phòng 101, 102) có giá 600.000 VND/đêm.
- Phòng tiêu chuẩn: Hạng Double (phòng 201, 202) có giá 950.000 VND/đêm.
- Phòng đắt nhất và cao cấp nhất: Hạng Suite (phòng 301) có giá 2.200.000 VND/đêm, miễn phí dịch vụ đưa đón sân bay Nội Bài.
- Giờ nhận phòng (check-in): từ 14:00. Giờ trả phòng (check-out): trước 12:00. Bữa sáng miễn phí: 6:30 - 9:30 hàng ngày.

CÁC HÀM CÔNG CỤ C# TRUY VẤN VÀ CẬP NHẬT CSDL THỰC TẾ (FUNCTION CALLING):
1. CheckAvailability(roomType, checkIn, checkOut):
   - Kiểm tra phòng trống thực tế trong CSDL và tính giá cho khoảng ngày từ checkIn đến checkOut (định dạng YYYY-MM-DD).
   - Nếu khách hỏi về 'hôm nay' để đặt phòng hoặc kiểm tra trống, mặc định checkIn = '{todayIso}', checkOut = '{tomorrowIso}'.
   - Nếu trước đó khách đang hỏi về một loại phòng (ví dụ phòng Suite) và ở câu tiếp theo khách cung cấp ngày (ví dụ '06-11-2026' hay '06/11/2026 đến 08/11/2026'), hãy tổng hợp ngữ cảnh và gọi hàm CheckAvailability với loại phòng đó.
2. GetBookingsByPhone(phone): Tra cứu toàn bộ lịch sử booking đã đặt trong CSDL theo số điện thoại khách hàng cung cấp.
3. CancelBooking(bookingId): Hủy booking trong CSDL theo mã bookingId.

QUY TẮC ĐỐI THOẠI BẮT BUỘC:
- DUY TRÌ NGỮ CẢNH: Đây là một cuộc trò chuyện liên tục. Hãy đọc kỹ lịch sử trao đổi trước đó để hiểu khách đang tiếp tục câu chuyện gì (ví dụ khách vừa cung cấp ngày sau khi bạn hỏi ngày checkout). Tuyệt đối không đối xử với câu trả lời của khách như một chủ đề hoàn toàn mới tách biệt.
- Trả lời về giá niêm yết: Khi khách hỏi về giá niêm yết, giá phòng hôm nay, phòng nào đắt nhất hay rẻ nhất, hãy trả lời ngay thông tin từ bảng Room ở trên một cách chính xác và nhiệt tình, sau đó hỏi khách có muốn kiểm tra tình trạng trống trong ngày cụ thể không.
- Khi khách muốn kiểm tra phòng trống hoặc đặt phòng, hãy gọi hàm CheckAvailability.
- Khi nhận được kết quả từ hàm C#, hãy giải thích rõ ràng và tự nhiên bằng tiếng Việt.";

                var toolsDeclaration = new JsonArray
                {
                    new JsonObject
                    {
                        ["functionDeclarations"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["name"] = "CheckAvailability",
                                ["description"] = "Return available rooms and price",
                                ["parameters"] = new JsonObject
                                {
                                    ["type"] = "OBJECT",
                                    ["properties"] = new JsonObject
                                    {
                                        ["roomType"] = new JsonObject { ["type"] = "STRING", ["description"] = "Type of room: Single, Double, Suite" },
                                        ["checkIn"] = new JsonObject { ["type"] = "STRING", ["description"] = "Check-in date in YYYY-MM-DD format" },
                                        ["checkOut"] = new JsonObject { ["type"] = "STRING", ["description"] = "Check-out date in YYYY-MM-DD format" }
                                    },
                                    ["required"] = new JsonArray { "roomType", "checkIn", "checkOut" }
                                }
                            },
                            new JsonObject
                            {
                                ["name"] = "GetBookingsByPhone",
                                ["description"] = "Return the bookings of a guest",
                                ["parameters"] = new JsonObject
                                {
                                    ["type"] = "OBJECT",
                                    ["properties"] = new JsonObject
                                    {
                                        ["phone"] = new JsonObject { ["type"] = "STRING", ["description"] = "Guest phone number" }
                                    },
                                    ["required"] = new JsonArray { "phone" }
                                }
                            },
                            new JsonObject
                            {
                                ["name"] = "CancelBooking",
                                ["description"] = "Change booking status to Cancelled",
                                ["parameters"] = new JsonObject
                                {
                                    ["type"] = "OBJECT",
                                    ["properties"] = new JsonObject
                                    {
                                        ["bookingId"] = new JsonObject { ["type"] = "INTEGER", ["description"] = "Booking identifier" }
                                    },
                                    ["required"] = new JsonArray { "bookingId" }
                                }
                            }
                        }
                    }
                };

                var contents = new JsonArray();
                if (history != null && history.Count > 0)
                {
                    string? lastRole = null;
                    foreach (var item in history)
                    {
                        if (string.IsNullOrWhiteSpace(item.Text)) continue;
                        var r = (item.Role?.ToLower() == "assistant" || item.Role?.ToLower() == "model") ? "model" : "user";
                        if (contents.Count == 0 && r != "user") continue;

                        if (r == lastRole && contents.Count > 0)
                        {
                            var prevObj = contents[contents.Count - 1] as JsonObject;
                            var partsArr = prevObj?["parts"] as JsonArray;
                            if (partsArr != null && partsArr.Count > 0)
                            {
                                var prevText = partsArr[0]?["text"]?.ToString() ?? string.Empty;
                                partsArr[0] = new JsonObject { ["text"] = prevText + "\n" + item.Text };
                            }
                        }
                        else
                        {
                            contents.Add(new JsonObject
                            {
                                ["role"] = r,
                                ["parts"] = new JsonArray { new JsonObject { ["text"] = item.Text } }
                            });
                            lastRole = r;
                        }
                    }

                    if (contents.Count > 0 && lastRole == "user")
                    {
                        var prevObj = contents[contents.Count - 1] as JsonObject;
                        var partsArr = prevObj?["parts"] as JsonArray;
                        if (partsArr != null && partsArr.Count > 0)
                        {
                            var prevText = partsArr[0]?["text"]?.ToString() ?? string.Empty;
                            partsArr[0] = new JsonObject { ["text"] = prevText + "\n" + message };
                        }
                    }
                    else
                    {
                        contents.Add(new JsonObject
                        {
                            ["role"] = "user",
                            ["parts"] = new JsonArray { new JsonObject { ["text"] = message } }
                        });
                    }
                }
                else
                {
                    contents.Add(new JsonObject
                    {
                        ["role"] = "user",
                        ["parts"] = new JsonArray { new JsonObject { ["text"] = message } }
                    });
                }

                var initialPayload = new JsonObject
                {
                    ["system_instruction"] = new JsonObject
                    {
                        ["parts"] = new JsonArray { new JsonObject { ["text"] = systemPrompt } }
                    },
                    ["contents"] = contents,
                    ["tools"] = toolsDeclaration
                };

                var responseJson = await SendGeminiRequestWithRetryAsync(initialPayload, apiKey);
                var functionCallPart = FindFunctionCallPart(responseJson);

                if (functionCallPart == null)
                {
                    var directText = ExtractTextFromGeminiResponse(responseJson);
                    if (!string.IsNullOrWhiteSpace(directText))
                    {
                        return new AssistantChatResponse { Response = directText };
                    }
                }
                else
                {
                    var functionName = functionCallPart["name"]?.ToString() ?? string.Empty;
                    var functionArgs = functionCallPart["args"] as JsonObject ?? new JsonObject();

                    object? toolExecutionResult = null;
                    if (functionName.Equals("CheckAvailability", StringComparison.OrdinalIgnoreCase))
                    {
                        var rType = functionArgs["roomType"]?.ToString() ?? string.Empty;
                        var cIn = functionArgs["checkIn"]?.ToString() ?? string.Empty;
                        var cOut = functionArgs["checkOut"]?.ToString() ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(cIn)) cIn = todayIso;
                        if (string.IsNullOrWhiteSpace(cOut)) cOut = tomorrowIso;
                        toolExecutionResult = await bookingService.CheckAvailabilityAsync(rType, cIn, cOut);
                    }
                    else if (functionName.Equals("GetBookingsByPhone", StringComparison.OrdinalIgnoreCase))
                    {
                        var phone = functionArgs["phone"]?.ToString() ?? string.Empty;
                        toolExecutionResult = await bookingService.GetBookingsByPhoneAsync(phone);
                    }
                    else if (functionName.Equals("CancelBooking", StringComparison.OrdinalIgnoreCase))
                    {
                        var bookingIdStr = functionArgs["bookingId"]?.ToString() ?? "0";
                        int.TryParse(bookingIdStr, out var bookingId);
                        toolExecutionResult = await bookingService.CancelBookingAsync(bookingId);
                    }

                    var modelContentNode = responseJson["candidates"]?[0]?["content"];
                    var modelTurn = JsonNode.Parse(modelContentNode?.ToJsonString() ?? "{}");

                    var toolResultNode = JsonNode.Parse(JsonSerializer.Serialize(toolExecutionResult));
                    var functionTurn = new JsonObject
                    {
                        ["role"] = "user",
                        ["parts"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["functionResponse"] = new JsonObject
                                {
                                    ["name"] = functionName,
                                    ["response"] = new JsonObject { ["result"] = toolResultNode }
                                }
                            }
                        }
                    };

                    var secondContents = JsonNode.Parse(contents.ToJsonString()) as JsonArray ?? new JsonArray();
                    secondContents.Add(modelTurn);
                    secondContents.Add(functionTurn);

                    var secondPayload = new JsonObject
                    {
                        ["system_instruction"] = new JsonObject
                        {
                            ["parts"] = new JsonArray { new JsonObject { ["text"] = systemPrompt } }
                        },
                        ["contents"] = secondContents,
                        ["tools"] = JsonNode.Parse(toolsDeclaration.ToJsonString())
                    };

                    var secondResponseJson = await SendGeminiRequestWithRetryAsync(secondPayload, apiKey);
                    var finalText = ExtractTextFromGeminiResponse(secondResponseJson);

                    return new AssistantChatResponse
                    {
                        Response = string.IsNullOrWhiteSpace(finalText) ? "Em đã xử lý xong yêu cầu của Quý khách." : finalText,
                        ToolCalled = functionName,
                        ToolResult = toolExecutionResult
                    };
                }
            }
            catch
            {
            }
        }

        return await FallbackAssistantChatAsync(message, bookingService, allRooms, history);
    }

    private async Task<JsonNode> SendGeminiRequestWithRetryAsync(JsonObject payload, string apiKey)
    {
        var cleanKey = CleanKey(apiKey);
        HttpResponseMessage? lastResponse = null;
        string lastContent = string.Empty;

        foreach (var model in _modelCandidates)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={cleanKey}";

            for (int attempt = 0; attempt < 2; attempt++)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json");

                try
                {
                    var response = await _httpClient.SendAsync(request);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var parsed = JsonNode.Parse(responseBody);
                        if (parsed != null)
                        {
                            return parsed;
                        }
                    }

                    lastResponse = response;
                    lastContent = responseBody;

                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        await Task.Delay(1000 * (attempt + 1));
                        continue;
                    }

                    if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        break;
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        if (lastResponse?.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new HttpRequestException("Gemini API đạt giới hạn hạn ngạch (429 Too Many Requests). Vui lòng thử lại sau giây lát.");
        }

        throw new HttpRequestException($"Gọi Gemini API không thành công: {lastContent}");
    }

    private static JsonObject? FindFunctionCallPart(JsonNode responseNode)
    {
        try
        {
            var candidates = responseNode["candidates"]?.AsArray();
            if (candidates == null || candidates.Count == 0) return null;

            var parts = candidates[0]?["content"]?["parts"]?.AsArray();
            if (parts == null) return null;

            foreach (var part in parts)
            {
                var functionCall = part?["functionCall"] as JsonObject;
                if (functionCall != null)
                {
                    return functionCall;
                }
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    private static string ExtractTextFromGeminiResponse(JsonNode responseNode)
    {
        try
        {
            var candidates = responseNode["candidates"]?.AsArray();
            if (candidates == null || candidates.Count == 0) return string.Empty;

            var parts = candidates[0]?["content"]?["parts"]?.AsArray();
            if (parts == null) return string.Empty;

            var sb = new StringBuilder();
            foreach (var part in parts)
            {
                var text = part?["text"]?.ToString();
                if (!string.IsNullOrEmpty(text))
                {
                    sb.Append(text);
                }
            }
            return sb.ToString().Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string CleanJsonString(string input)
    {
        var trimmed = input.Trim();
        if (trimmed.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed.Substring(7);
        }
        else if (trimmed.StartsWith("```"))
        {
            trimmed = trimmed.Substring(3);
        }

        if (trimmed.EndsWith("```"))
        {
            trimmed = trimmed.Substring(0, trimmed.Length - 3);
        }

        return trimmed.Trim();
    }

    private static string FallbackAskHotelResponse(string question)
    {
        var q = question.ToLower();

        if (q.Contains("check-in") || q.Contains("checkin") || q.Contains("nhận phòng") || q.Contains("nhan phong"))
        {
            return "Giờ nhận phòng (check-in) tại Sunrise Hotel là từ 14:00. Giờ trả phòng (check-out) là trước 12:00 trưa Quý khách nhé.";
        }
        if (q.Contains("check-out") || q.Contains("checkout") || q.Contains("trả phòng") || q.Contains("tra phong"))
        {
            return "Giờ trả phòng (check-out) tại Sunrise Hotel quy định trước 12:00 trưa hàng ngày Quý khách nhé.";
        }
        if (q.Contains("sáng") || q.Contains("sang") || q.Contains("breakfast") || q.Contains("ăn sáng"))
        {
            return "Sunrise Hotel phục vụ bữa sáng hoàn toàn miễn phí cho khách lưu trú từ 6:30 đến 9:30 hàng ngày tại nhà hàng của khách sạn ạ.";
        }
        if (q.Contains("bơi") || q.Contains("boi") || q.Contains("pool") || q.Contains("hồ bơi"))
        {
            return "Khách sạn Sunrise Hotel có hồ bơi nằm tại tầng 5 với view ngắm toàn cảnh thành phố, mở cửa miễn phí cho khách lưu trú ạ.";
        }
        if (q.Contains("sân bay") || q.Contains("san bay") || q.Contains("đưa đón") || q.Contains("airport"))
        {
            return "Sunrise Hotel có dịch vụ đón tiễn sân bay Nội Bài hoàn toàn miễn phí dành riêng cho khách đặt phòng hạng Suite Quý khách nhé.";
        }
        if (q.Contains("giá") || q.Contains("gia") || q.Contains("phòng") || q.Contains("loại phòng"))
        {
            return "Bảng giá phòng niêm yết tại Sunrise Hotel:\n- Phòng Single (Đơn): 600.000 VND / đêm (tối đa 1 khách)\n- Phòng Double (Đôi): 950.000 VND / đêm (tối đa 2 khách)\n- Phòng Suite (Cao cấp): 2.200.000 VND / đêm (tối đa 4 khách, miễn phí đón sân bay).";
        }
        if (q.Contains("địa chỉ") || q.Contains("dia chi") || q.Contains("ở đâu") || q.Contains("o dau"))
        {
            return "Khách sạn Sunrise Hotel tọa lạc tại địa chỉ: 25 Trần Phú, Hà Nội rất thuận tiện di chuyển tham quan và làm việc ạ.";
        }

        if (q.Contains("thời tiết") || q.Contains("chính trị") || q.Contains("toán") || q.Contains("sa pa") || q.Contains("tokyo") || q.Contains("hà lội") || q.Contains("hồ chí minh"))
        {
            return "Xin lỗi Quý khách! Em là nhân viên lễ tân của Sunrise Hotel (25 Trần Phú, Hà Nội) nên chỉ có thể hỗ trợ các thông tin liên quan đến dịch vụ và tiện ích của khách sạn. Rất mong Quý khách thông cảm ạ.";
        }

        return "Sunrise Hotel xin kính chào Quý khách! Khách sạn có địa chỉ tại 25 Trần Phú, Hà Nội; giờ nhận phòng từ 14:00, trả phòng trước 12:00, ăn sáng miễn phí 6:30 - 9:30 và hồ bơi tại tầng 5. Em có thể hỗ trợ thêm thông tin gì về khách sạn cho Quý khách ạ?";
    }

    private static ParsedBookingDto FallbackParseBooking(string text)
    {
        var dto = new ParsedBookingDto();

        var phoneMatch = Regex.Match(text, @"(0[3|5|7|8|9][0-9]{8}|090[0-9]{7}|091[0-9]{7}|098[0-9]{7})");
        if (phoneMatch.Success)
        {
            dto.Phone = phoneMatch.Value;
        }

        var nameMatch = Regex.Match(text, @"(?:Tôi là|Mình là|Tên tôi là|tên là)\s+([A-ZÀ-Ỹa-zà-ỹ\s]+?)(?:,|\s+sđt|\s+số|\s+phone|\s+muốn|\s+cần)", RegexOptions.IgnoreCase);
        if (nameMatch.Success)
        {
            dto.GuestName = nameMatch.Groups[1].Value.Trim();
        }
        else
        {
            dto.GuestName = "Khách Hàng";
        }

        var lower = text.ToLower();
        if (lower.Contains("suite")) dto.RoomType = "Suite";
        else if (lower.Contains("đôi") || lower.Contains("doi") || lower.Contains("double")) dto.RoomType = "Double";
        else dto.RoomType = "Single";

        var dateMatches = Regex.Matches(text, @"(\d{1,2})[/.-](\d{1,2})(?:[/.-](\d{4}))?");
        if (dateMatches.Count >= 2)
        {
            var d1 = int.Parse(dateMatches[0].Groups[1].Value);
            var m1 = int.Parse(dateMatches[0].Groups[2].Value);
            var y1 = dateMatches[0].Groups[3].Success ? int.Parse(dateMatches[0].Groups[3].Value) : 2026;

            var d2 = int.Parse(dateMatches[1].Groups[1].Value);
            var m2 = int.Parse(dateMatches[1].Groups[2].Value);
            var y2 = dateMatches[1].Groups[3].Success ? int.Parse(dateMatches[1].Groups[3].Value) : 2026;

            dto.CheckIn = $"{y1:D4}-{m1:D2}-{d1:D2}";
            dto.CheckOut = $"{y2:D4}-{m2:D2}-{d2:D2}";
        }
        else
        {
            dto.CheckIn = "2026-11-20";
            dto.CheckOut = "2026-11-22";
        }

        var guestsMatch = Regex.Match(text, @"(\d+)\s*(?:người|khách|nguoi|khach)", RegexOptions.IgnoreCase);
        if (guestsMatch.Success)
        {
            dto.Guests = int.Parse(guestsMatch.Groups[1].Value);
        }
        else
        {
            dto.Guests = dto.RoomType == "Single" ? 1 : (dto.RoomType == "Double" ? 2 : 4);
        }

        return dto;
    }

    private async Task<AssistantChatResponse> FallbackAssistantChatAsync(
        string message,
        IBookingService bookingService,
        List<Room>? rooms = null,
        List<ChatHistoryItem>? history = null)
    {
        var lower = message.ToLower();

        if (lower.Contains("đắt nhất") || lower.Contains("cao nhất") || (lower.Contains("giá") && lower.Contains("suite")))
        {
            var suite = rooms?.FirstOrDefault(r => r.RoomType == "Suite");
            var priceText = suite != null ? $"{suite.PricePerNight:N0} VND" : "2.200.000 VND";
            return new AssistantChatResponse
            {
                Response = $"Dạ chào Quý khách! Hiện tại khách sạn Sunrise Hotel có hạng phòng cao cấp và đắt nhất là **Phòng Suite (phòng 301)** với giá niêm yết là **{priceText}/đêm** (dành cho tối đa 4 khách và có xe đưa đón sân bay Nội Bài miễn phí). Quý khách muốn kiểm tra phòng trống cho ngày nào ạ?"
            };
        }

        if (lower.Contains("rẻ nhất") || lower.Contains("thấp nhất"))
        {
            var single = rooms?.FirstOrDefault(r => r.RoomType == "Single");
            var priceText = single != null ? $"{single.PricePerNight:N0} VND" : "600.000 VND";
            return new AssistantChatResponse
            {
                Response = $"Dạ chào Quý khách! Hạng phòng tiết kiệm nhất của khách sạn là **Phòng Single (Đơn - phòng 101, 102)** với giá niêm yết là **{priceText}/đêm** (dành cho 1 khách, gồm bữa sáng miễn phí) ạ."
            };
        }

        if (lower.Contains("bảng giá") || (lower.Contains("giá") && lower.Contains("phòng")))
        {
            return new AssistantChatResponse
            {
                Response = "Dạ bảng giá niêm yết các hạng phòng tại Sunrise Hotel như sau:\n- Phòng Single: 600.000 VND/đêm (phòng 101, 102)\n- Phòng Double: 950.000 VND/đêm (phòng 201, 202)\n- Phòng Suite: 2.200.000 VND/đêm (phòng 301, miễn phí đón sân bay Nội Bài).\nQuý khách muốn đặt hoặc kiểm tra phòng trống ngày nào ạ?"
            };
        }

        if (lower.Contains("hello") || lower.Contains("xin chào") || lower.Contains("chào") || lower.Contains("khoe") || lower.Contains("khỏe") || lower.Contains("hi ") || lower.Equals("hi"))
        {
            return new AssistantChatResponse
            {
                Response = "Dạ em chào Quý khách! Em là trợ lý ảo của khách sạn Sunrise Hotel (25 Trần Phú, Hà Nội). Em rất khỏe và sẵn sàng hỗ trợ Quý khách! Quý khách có thể hỏi em kiểm tra phòng trống, tra cứu lịch sử đặt phòng qua số điện thoại, hoặc hỗ trợ hủy đặt phòng theo mã booking ạ."
            };
        }

        if (lower.Contains("hủy") || lower.Contains("huy"))
        {
            var numMatch = Regex.Match(message, @"\d+");
            if (numMatch.Success && int.TryParse(numMatch.Value, out var bookingId))
            {
                var cancelResult = await bookingService.CancelBookingAsync(bookingId);
                return new AssistantChatResponse
                {
                    Response = cancelResult.Message,
                    ToolCalled = "CancelBooking",
                    ToolResult = cancelResult
                };
            }
            return new AssistantChatResponse
            {
                Response = "Quý khách vui lòng cung cấp mã số đặt phòng (ví dụ: 'Hủy giúp tôi booking số 3') để em hỗ trợ hủy phòng ngay ạ."
            };
        }

        var phoneMatch = Regex.Match(message, @"(0[3|5|7|8|9][0-9]{8}|090[0-9]{7}|091[0-9]{7}|098[0-9]{7})");
        if (phoneMatch.Success)
        {
            var phone = phoneMatch.Value;
            var bookings = await bookingService.GetBookingsByPhoneAsync(phone);
            if (bookings.Count == 0)
            {
                return new AssistantChatResponse
                {
                    Response = $"Dạ em đã kiểm tra hệ thống nhưng không tìm thấy booking nào của số điện thoại {phone} ạ.",
                    ToolCalled = "GetBookingsByPhone",
                    ToolResult = bookings
                };
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Dạ số điện thoại {phone} có {bookings.Count} booking trong hệ thống:");
            foreach (var b in bookings)
            {
                sb.AppendLine($"- Booking #{b.BookingId}: Phòng {b.RoomNumber} ({b.RoomType}), nhận ngày {b.CheckIn} đến {b.CheckOut}, tổng tiền {b.FormattedTotalPrice}, trạng thái: {b.Status}");
            }
            return new AssistantChatResponse
            {
                Response = sb.ToString().Trim(),
                ToolCalled = "GetBookingsByPhone",
                ToolResult = bookings
            };
        }

        if (lower.Contains("phòng") || lower.Contains("trống") || lower.Contains("còn") || lower.Contains("suite") || lower.Contains("single") || lower.Contains("double"))
        {
            var rType = "Suite";
            if (lower.Contains("đôi") || lower.Contains("double")) rType = "Double";
            else if (lower.Contains("đơn") || lower.Contains("single")) rType = "Single";

            var dateMatches = Regex.Matches(message, @"(\d{1,2})[/.-](\d{1,2})(?:[/.-](\d{4}))?");
            string checkIn = "2026-12-24";
            string checkOut = "2026-12-26";

            if (dateMatches.Count >= 2)
            {
                var d1 = int.Parse(dateMatches[0].Groups[1].Value);
                var m1 = int.Parse(dateMatches[0].Groups[2].Value);
                var y1 = dateMatches[0].Groups[3].Success ? int.Parse(dateMatches[0].Groups[3].Value) : 2026;

                var d2 = int.Parse(dateMatches[1].Groups[1].Value);
                var m2 = int.Parse(dateMatches[1].Groups[2].Value);
                var y2 = dateMatches[1].Groups[3].Success ? int.Parse(dateMatches[1].Groups[3].Value) : 2026;

                checkIn = $"{y1:D4}-{m1:D2}-{d1:D2}";
                checkOut = $"{y2:D4}-{m2:D2}-{d2:D2}";
            }

            var availableRooms = await bookingService.CheckAvailabilityAsync(rType, checkIn, checkOut);
            if (availableRooms.Count == 0)
            {
                return new AssistantChatResponse
                {
                    Response = $"Dạ trong khoảng thời gian từ {checkIn} đến {checkOut}, hiện tại không còn phòng {rType} nào trống ạ.",
                    ToolCalled = "CheckAvailability",
                    ToolResult = availableRooms
                };
            }

            var roomDetails = string.Join(", ", availableRooms.Select(r => $"Phòng {r.RoomNumber} ({r.FormattedPrice}/đêm, tối đa {r.MaxGuests} khách)"));
            return new AssistantChatResponse
            {
                Response = $"Dạ trong khoảng thời gian từ {checkIn} đến {checkOut}, khách sạn còn phòng {rType} trống: {roomDetails}. Quý khách có muốn tiến hành đặt phòng không ạ?",
                ToolCalled = "CheckAvailability",
                ToolResult = availableRooms
            };
        }

        return new AssistantChatResponse
        {
            Response = "Dạ em là trợ lý ảo của Sunrise Hotel tại 25 Trần Phú, Hà Nội. Em có thể giúp Quý khách: 1. Kiểm tra phòng trống theo loại phòng và ngày; 2. Tra cứu lịch sử đặt phòng qua SĐT; 3. Hỗ trợ hủy booking theo mã số. Quý khách cần em hỗ trợ việc gì ạ?"
        };
    }
}
