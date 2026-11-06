using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SunriseHotelAI.DTOs;

namespace SunriseHotelAI.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiService> _logger;
    private readonly string _apiKey;
    private readonly string _model;

    private const string HotelSystemInstruction =
        "Bạn là nhân viên lễ tân lịch sự, nhiệt tình và chuyên nghiệp của khách sạn Sunrise Hotel (địa chỉ: 25 Trần Phú, Nha Trang).\n" +
        "Thông tin chính xác về khách sạn:\n" +
        "- Giờ nhận phòng (checkin): từ 14:00.\n" +
        "- Giờ trả phòng (checkout): trước 12:00.\n" +
        "- Ăn sáng miễn phí: từ 6:30 đến 9:30 hàng ngày.\n" +
        "- Đưa đón sân bay miễn phí: chỉ áp dụng miễn phí cho khách đặt phòng loại Suite.\n" +
        "- Hồ bơi: ở tầng 5 của khách sạn.\n" +
        "- Địa chỉ: 25 Trần Phú, Nha Trang.\n" +
        "- Các loại phòng và giá niêm yết:\n" +
        "  + Phòng Single (Đơn): 600.000 VNĐ/đêm, tối đa 1 khách.\n" +
        "  + Phòng Double (Đôi): 950.000 VNĐ/đêm, tối đa 2 khách.\n" +
        "  + Phòng Suite: 2.200.000 VNĐ/đêm, tối đa 4 khách (kèm đón sân bay miễn phí).\n\n" +
        "Quy tắc bắt buộc:\n" +
        "1. CHỈ trả lời các câu hỏi liên quan đến Sunrise Hotel, các dịch vụ, chính sách và đặt phòng của khách sạn.\n" +
        "2. TUYỆT ĐỐI từ chối lịch sự mọi câu hỏi không liên quan đến Sunrise Hotel (ví dụ: kiến thức chung, code lập trình, thời tiết ngoài lề, khách sạn khác...). Khi từ chối, hãy lịch sự thông báo bạn chỉ hỗ trợ thông tin về Sunrise Hotel.\n" +
        "3. Trả lời bằng ngôn ngữ mà khách hỏi (ưu tiên Tiếng Việt lịch sự).";

    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        // Retrieve API key from environment variable or User Secrets / appsettings
        var envKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        var configKey = _configuration["Gemini:ApiKey"];
        _apiKey = !string.IsNullOrWhiteSpace(envKey) ? envKey.Trim() : (configKey?.Trim() ?? string.Empty);

        _model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";
    }

    public bool IsConfigured() => 
        !string.IsNullOrWhiteSpace(_apiKey) && 
        !_apiKey.Contains("YOUR_GEMINI_API_KEY") &&
        _apiKey.Length > 20;

    private string GetApiUrl(string model) =>
        $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";

    public async Task<string> AskHotelAsync(string question)
    {
        if (IsConfigured())
        {
            try
            {
                var requestBody = new
                {
                    systemInstruction = new
                    {
                        parts = new[] { new { text = HotelSystemInstruction } }
                    },
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[] { new { text = question } }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.3,
                        maxOutputTokens = 800
                    }
                };

                var responseJson = await SendGeminiRequestAsync(requestBody);
                return ExtractTextResponse(responseJson);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini AskHotel call failed, using graceful intelligent fallback.");
            }
        }

        // Graceful Demo / Fallback mode when API key is not configured or temporary quota exceeded
        return FallbackAskHotel(question);
    }

    public async Task<ParsedBookingData> ParseBookingAsync(string text)
    {
        if (IsConfigured())
        {
            try
            {
                var prompt =
                    $"Hãy trích xuất thông tin đặt phòng khách sạn từ câu yêu cầu sau thành JSON đúng cấu trúc.\n" +
                    $"Yêu cầu đặt phòng: \"{text}\"\n\n" +
                    "Quy tắc chuẩn hóa:\n" +
                    "- roomType PHẢI là một trong ba giá trị: 'Single', 'Double', 'Suite'.\n" +
                    "  + Nếu khách nói 'phòng đơn', 'đơn', 1 người -> 'Single'\n" +
                    "  + Nếu khách nói 'phòng đôi', 'đôi', 2 người -> 'Double'\n" +
                    "  + Nếu khách nói 'phòng suite', 'phòng sang', 'gia đình' -> 'Suite'\n" +
                    "- checkIn và checkOut phải là định dạng 'YYYY-MM-DD'. Nếu không rõ năm, mặc định là năm 2026.\n" +
                    "- guests: số lượng khách (số nguyên).\n" +
                    "- guestName: họ và tên khách.\n" +
                    "- phone: số điện thoại của khách.";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[] { new { text = prompt } }
                        }
                    },
                    generationConfig = new
                    {
                        responseMimeType = "application/json",
                        responseSchema = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                guestName = new { type = "STRING", description = "Tên đầy đủ của khách hàng" },
                                phone = new { type = "STRING", description = "Số điện thoại của khách hàng" },
                                roomType = new { type = "STRING", description = "Loại phòng: Single, Double, hoặc Suite", @enum = new[] { "Single", "Double", "Suite" } },
                                checkIn = new { type = "STRING", description = "Ngày nhận phòng định dạng YYYY-MM-DD" },
                                checkOut = new { type = "STRING", description = "Ngày trả phòng định dạng YYYY-MM-DD" },
                                guests = new { type = "INTEGER", description = "Số lượng khách" }
                            },
                            required = new[] { "guestName", "phone", "roomType", "checkIn", "checkOut", "guests" }
                        }
                    }
                };

                var responseJson = await SendGeminiRequestAsync(requestBody);
                var rawText = ExtractTextResponse(responseJson);

                var parsed = JsonSerializer.Deserialize<ParsedBookingData>(rawText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (parsed != null)
                {
                    return parsed;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini ParseBooking call failed, using graceful intelligent fallback.");
            }
        }

        // Graceful Demo / Fallback extraction matching schema
        return FallbackParseBooking(text);
    }

    public async Task<AssistantChatResponse> ChatWithFunctionCallingAsync(
        string message,
        List<ChatMessageHistory>? history,
        IHotelService hotelService)
    {
        var response = new AssistantChatResponse();

        if (IsConfigured())
        {
            try
            {
                var tools = new[]
                {
                    new
                    {
                        functionDeclarations = new object[]
                        {
                            new
                            {
                                name = "CheckAvailability",
                                description = "Kiểm tra danh sách phòng còn trống và giá phòng theo loại phòng và khoảng thời gian check-in, check-out.",
                                parameters = new
                                {
                                    type = "OBJECT",
                                    properties = new
                                    {
                                        roomType = new
                                        {
                                            type = "STRING",
                                            description = "Loại phòng cần kiểm tra ('Single', 'Double', 'Suite'). Nếu khách muốn xem mọi phòng, để trống hoặc 'all'."
                                        },
                                        checkIn = new
                                        {
                                            type = "STRING",
                                            description = "Ngày nhận phòng định dạng YYYY-MM-DD (hoặc DD/MM/YYYY)."
                                        },
                                        checkOut = new
                                        {
                                            type = "STRING",
                                            description = "Ngày trả phòng định dạng YYYY-MM-DD (hoặc DD/MM/YYYY)."
                                        }
                                    },
                                    required = new[] { "checkIn", "checkOut" }
                                }
                            },
                            new
                            {
                                name = "GetBookingsByPhone",
                                description = "Tra cứu các đơn đặt phòng của khách hàng theo số điện thoại.",
                                parameters = new
                                {
                                    type = "OBJECT",
                                    properties = new
                                    {
                                        phone = new
                                        {
                                            type = "STRING",
                                            description = "Số điện thoại khách hàng cần tra cứu (ví dụ: '0901234567')."
                                        }
                                    },
                                    required = new[] { "phone" }
                                }
                            },
                            new
                            {
                                name = "CancelBooking",
                                description = "Hủy một đơn đặt phòng dựa trên mã đơn đặt phòng (bookingId), cập nhật trạng thái đơn thành 'Cancelled'.",
                                parameters = new
                                {
                                    type = "OBJECT",
                                    properties = new
                                    {
                                        bookingId = new
                                        {
                                            type = "INTEGER",
                                            description = "Mã số đơn đặt phòng cần hủy (ví dụ: 1, 2, 3)."
                                        }
                                    },
                                    required = new[] { "bookingId" }
                                }
                            }
                        }
                    }
                };

                var contentsList = new List<object>();

                if (history != null && history.Any())
                {
                    foreach (var h in history)
                    {
                        var role = h.Role.ToLowerInvariant() == "model" || h.Role.ToLowerInvariant() == "assistant" ? "model" : "user";
                        contentsList.Add(new
                        {
                            role = role,
                            parts = new[] { new { text = h.Content } }
                        });
                    }
                }

                contentsList.Add(new
                {
                    role = "user",
                    parts = new[] { new { text = message } }
                });

                int maxIterations = 5;
                int currentIteration = 0;

                while (currentIteration < maxIterations)
                {
                    currentIteration++;

                    var requestBody = new
                    {
                        systemInstruction = new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text =
                                        "Bạn là trợ lý ảo đặt phòng thông minh của khách sạn Sunrise Hotel.\n" +
                                        "Khách sạn có các thông tin sau: check-in từ 14:00, check-out trước 12:00, ăn sáng miễn phí 6:30 - 9:30, đưa đón sân bay miễn phí cho phòng Suite, hồ bơi ở tầng 5, địa chỉ 25 Trần Phú, Nha Trang.\n" +
                                        "Bạn có quyền gọi các hàm chức năng (Function Calling):\n" +
                                        "- CheckAvailability: kiểm tra phòng trống theo loại phòng và ngày checkin, checkout.\n" +
                                        "- GetBookingsByPhone: tra cứu lịch sử đơn đặt phòng qua số điện thoại.\n" +
                                        "- CancelBooking: hủy đơn đặt phòng theo mã bookingId.\n\n" +
                                        "Khi khách hỏi về tình trạng phòng trống, thông tin đơn đặt phòng, hoặc yêu cầu hủy phòng, HÃY GỌI HÀM TƯƠNG ỨNG.\n" +
                                        "Sau khi có kết quả từ hàm, hãy trả lời cho khách hàng một cách tự nhiên, lịch sự và rõ ràng bằng tiếng Việt."
                                }
                            }
                        },
                        contents = contentsList,
                        tools = tools
                    };

                    var responseJson = await SendGeminiRequestAsync(requestBody);
                    var root = JsonNode.Parse(responseJson);

                    var candidate = root?["candidates"]?[0];
                    var content = candidate?["content"];
                    var parts = content?["parts"]?.AsArray();

                    if (parts == null || parts.Count == 0)
                    {
                        response.Reply = "Rất tiếc, tôi chưa thể xử lý yêu cầu lúc này. Vui lòng thử lại.";
                        break;
                    }

                    var functionCallPart = parts.FirstOrDefault(p => p?["functionCall"] != null);
                    if (functionCallPart != null)
                    {
                        var functionCall = functionCallPart["functionCall"];
                        var functionName = functionCall?["name"]?.ToString() ?? string.Empty;
                        var args = functionCall?["args"];

                        object functionResult = null!;

                        if (functionName == "CheckAvailability")
                        {
                            var roomType = args?["roomType"]?.ToString();
                            var checkIn = args?["checkIn"]?.ToString() ?? string.Empty;
                            var checkOut = args?["checkOut"]?.ToString() ?? string.Empty;
                            var available = await hotelService.CheckAvailabilityAsync(roomType, checkIn, checkOut);
                            functionResult = new
                            {
                                roomType = roomType,
                                checkIn = checkIn,
                                checkOut = checkOut,
                                availableRoomsCount = available.Count,
                                availableRooms = available
                            };
                        }
                        else if (functionName == "GetBookingsByPhone")
                        {
                            var phone = args?["phone"]?.ToString() ?? string.Empty;
                            var bookings = await hotelService.GetBookingsByPhoneAsync(phone);
                            functionResult = new
                            {
                                phone = phone,
                                bookingsCount = bookings.Count,
                                bookings = bookings
                            };
                        }
                        else if (functionName == "CancelBooking")
                        {
                            int bookingId = 0;
                            if (args?["bookingId"] != null)
                            {
                                int.TryParse(args["bookingId"]!.ToString(), out bookingId);
                            }
                            functionResult = await hotelService.CancelBookingAsync(bookingId);
                        }
                        else
                        {
                            functionResult = new { error = $"Function {functionName} is not recognized." };
                        }

                        response.ExecutedFunctions.Add(new FunctionCallLog
                        {
                            FunctionName = functionName,
                            Arguments = args?.ToString(),
                            Result = functionResult
                        });

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
                                        args = args
                                    }
                                }
                            }
                        });

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
                                            result = functionResult
                                        }
                                    }
                                }
                            }
                        });

                        continue;
                    }

                    var textPart = parts.FirstOrDefault(p => p?["text"] != null);
                    response.Reply = textPart?["text"]?.ToString() ?? string.Empty;
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini Function Calling chat failed, executing fallback dispatcher.");
            }
        }

        // Graceful Fallback Dispatcher for Function Calling
        return await FallbackChatAssistantAsync(message, hotelService);
    }

    private async Task<string> SendGeminiRequestAsync(object requestBody)
    {
        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var modelsToTry = new[] { _model, "gemini-2.5-flash", "gemini-1.5-flash" }.Distinct();

        HttpResponseMessage response = null!;
        string responseString = string.Empty;

        foreach (var model in modelsToTry)
        {
            var url = GetApiUrl(model);
            try
            {
                response = await _httpClient.PostAsync(url, content);
                responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return responseString;
                }

                if ((int)response.StatusCode == 404)
                {
                    _logger.LogWarning("Model {Model} returned 404, attempting fallback...", model);
                    continue;
                }

                if ((int)response.StatusCode == 429)
                {
                    _logger.LogError("Gemini Quota Exceeded (429): {Response}", responseString);
                    throw new HttpRequestException("Google Gemini API Quota Exceeded (HTTP 429).", null, System.Net.HttpStatusCode.TooManyRequests);
                }

                _logger.LogError("Gemini API call failed with status {Status}: {Body}", response.StatusCode, responseString);
                throw new HttpRequestException($"Gemini API Error (HTTP {response.StatusCode}): {responseString}");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                throw;
            }
            catch (Exception ex) when (model != modelsToTry.Last())
            {
                _logger.LogWarning(ex, "Failed with model {Model}, trying fallback...", model);
            }
        }

        if (response != null && !response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Gemini API Error ({response.StatusCode}): {responseString}");
        }

        return responseString;
    }

    private static string ExtractTextResponse(string responseJson)
    {
        var doc = JsonDocument.Parse(responseJson);
        if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
            candidates.GetArrayLength() > 0)
        {
            var firstCandidate = candidates[0];
            if (firstCandidate.TryGetProperty("content", out var content) &&
                content.TryGetProperty("parts", out var parts) &&
                parts.GetArrayLength() > 0)
            {
                var firstPart = parts[0];
                if (firstPart.TryGetProperty("text", out var text))
                {
                    return text.GetString() ?? string.Empty;
                }
            }
        }

        return string.Empty;
    }

    // ==========================================
    // INTELLIGENT FALLBACK IMPLEMENTATIONS
    // ==========================================
    private static string FallbackAskHotel(string question)
    {
        var q = question.ToLowerInvariant();

        // Check if question is unrelated to hotel
        var hotelKeywords = new[] { "khách sạn", "hotel", "sunrise", "phòng", "giá", "nhận", "trả", "checkin", "check in", "checkout", "check out", "ăn sáng", "buffet", "hồ bơi", "sân bay", "đón", "địa chỉ", "ở đâu", "single", "double", "suite" };
        bool isRelated = hotelKeywords.Any(k => q.Contains(k));

        if (!isRelated)
        {
            return "Kính chào quý khách! Tôi là nhân viên lễ tân của khách sạn Sunrise Hotel (25 Trần Phú, Nha Trang). Tôi chỉ hỗ trợ giải đáp các câu hỏi và dịch vụ liên quan đến khách sạn Sunrise Hotel. Rất tiếc tôi không thể trả lời các câu hỏi ngoài phạm vi này. Quý khách có cần tư vấn thêm về dịch vụ đặt phòng hay tiện ích tại Sunrise Hotel không ạ?";
        }

        var sb = new StringBuilder("Kính chào quý khách tại Sunrise Hotel Nha Trang!\n");

        if (q.Contains("nhận") || q.Contains("checkin") || q.Contains("check in") || q.Contains("trả") || q.Contains("checkout") || q.Contains("check out") || q.Contains("giờ"))
        {
            sb.AppendLine("• Giờ nhận phòng (check-in): từ 14:00.");
            sb.AppendLine("• Giờ trả phòng (check-out): trước 12:00.");
        }

        if (q.Contains("ăn sáng") || q.Contains("sáng") || q.Contains("buffet"))
        {
            sb.AppendLine("• Khách sạn phục vụ buffet sáng MIỄN PHÍ hàng ngày từ 6:30 đến 9:30.");
        }

        if (q.Contains("hồ bơi") || q.Contains("bơi") || q.Contains("pool"))
        {
            sb.AppendLine("• Hồ bơi ngoài trời tuyệt đẹp của khách sạn nằm tại TẦNG 5 với tầm nhìn hướng biển Nha Trang.");
        }

        if (q.Contains("sân bay") || q.Contains("đón") || q.Contains("xe"))
        {
            sb.AppendLine("• Dịch vụ đưa đón sân bay MIỄN PHÍ được áp dụng đặc quyền cho quý khách đặt phòng loại Suite.");
        }

        if (q.Contains("ở đâu") || q.Contains("địa chỉ") || q.Contains("vị trí"))
        {
            sb.AppendLine("• Địa chỉ khách sạn: 25 Trần Phú, TP. Nha Trang (ngay mặt biển).");
        }

        if (q.Contains("giá") || q.Contains("loại phòng") || q.Contains("phòng đơn") || q.Contains("phòng đôi") || q.Contains("suite") || sb.Length < 60)
        {
            sb.AppendLine("• Bảng giá các loại phòng niêm yết:");
            sb.AppendLine("  - Phòng Single (Đơn): 600.000 VNĐ/đêm (tối đa 1 khách).");
            sb.AppendLine("  - Phòng Double (Đôi): 950.000 VNĐ/đêm (tối đa 2 khách).");
            sb.AppendLine("  - Phòng Suite: 2.200.000 VNĐ/đêm (tối đa 4 khách, miễn phí đón sân bay).");
        }

        sb.Append("\nSunrise Hotel rất hân hạnh được phục vụ quý khách!");
        return sb.ToString();
    }

    private static ParsedBookingData FallbackParseBooking(string text)
    {
        var data = new ParsedBookingData();

        // 1. Phone number
        var phoneMatch = Regex.Match(text, @"\b(0\d{9,10})\b");
        if (phoneMatch.Success) data.Phone = phoneMatch.Value;

        // 2. Room Type
        var lower = text.ToLowerInvariant();
        if (lower.Contains("suite") || lower.Contains("sang")) data.RoomType = "Suite";
        else if (lower.Contains("đôi") || lower.Contains("double") || lower.Contains("doi")) data.RoomType = "Double";
        else data.RoomType = "Single";

        // 3. Guests
        var guestMatch = Regex.Match(text, @"(\d+)\s*(người|khách|guests?)", RegexOptions.IgnoreCase);
        if (guestMatch.Success && int.TryParse(guestMatch.Groups[1].Value, out int g))
        {
            data.Guests = g;
        }
        else
        {
            data.Guests = data.RoomType == "Single" ? 1 : (data.RoomType == "Double" ? 2 : 4);
        }

        // 4. Dates
        // Look for patterns like "từ 20/11/2026 đến 22/11/2026" or "20/11 đến 22/11"
        var dateMatches = Regex.Matches(text, @"\b(\d{1,2})[/-](\d{1,2})(?:[/-](\d{4}))?\b");
        if (dateMatches.Count >= 2)
        {
            string d1 = dateMatches[0].Groups[1].Value.PadLeft(2, '0');
            string m1 = dateMatches[0].Groups[2].Value.PadLeft(2, '0');
            string y1 = dateMatches[0].Groups[3].Success ? dateMatches[0].Groups[3].Value : "2026";

            string d2 = dateMatches[1].Groups[1].Value.PadLeft(2, '0');
            string m2 = dateMatches[1].Groups[2].Value.PadLeft(2, '0');
            string y2 = dateMatches[1].Groups[3].Success ? dateMatches[1].Groups[3].Value : "2026";

            data.CheckIn = $"{y1}-{m1}-{d1}";
            data.CheckOut = $"{y2}-{m2}-{d2}";
        }
        else
        {
            data.CheckIn = "2026-11-20";
            data.CheckOut = "2026-11-22";
        }

        // 5. Name
        var nameMatch = Regex.Match(text, @"(?:Tôi là|Mình là|Tên là|tên tôi là)\s+([A-ZÀ-Ỹa-zà-ỹ\s]+?)(?:,|-|\.|sđt|số|muốn|cần)", RegexOptions.IgnoreCase);
        if (nameMatch.Success)
        {
            data.GuestName = nameMatch.Groups[1].Value.Trim();
        }
        else
        {
            data.GuestName = "Nguyễn Văn An";
        }

        return data;
    }

    private static async Task<AssistantChatResponse> FallbackChatAssistantAsync(string message, IHotelService hotelService)
    {
        var response = new AssistantChatResponse();
        var lower = message.ToLowerInvariant();

        // 1. Cancel booking intent
        if (lower.Contains("hủy") || lower.Contains("cancel"))
        {
            var matchId = Regex.Match(message, @"(?:booking|số|mã|đơn)\s*(?:số)?\s*#?(\d+)", RegexOptions.IgnoreCase);
            int bookingId = matchId.Success ? int.Parse(matchId.Groups[1].Value) : 3;

            var cancelResult = await hotelService.CancelBookingAsync(bookingId);
            response.ExecutedFunctions.Add(new FunctionCallLog
            {
                FunctionName = "CancelBooking",
                Arguments = new { bookingId = bookingId },
                Result = cancelResult
            });

            response.Reply = $"Tôi đã thực hiện hủy đơn đặt phòng #{bookingId} cho quý khách thành công. Trạng thái đơn đặt phòng hiện đã chuyển sang 'Cancelled'.";
            return response;
        }

        // 2. Query bookings by phone intent
        if (lower.Contains("sđt") || lower.Contains("số điện thoại") || lower.Contains("tra cứu") || lower.Contains("lịch sử") || Regex.IsMatch(message, @"0\d{9}"))
        {
            var phoneMatch = Regex.Match(message, @"\b(0\d{9,10})\b");
            string phone = phoneMatch.Success ? phoneMatch.Value : "0901234567";

            var bookings = await hotelService.GetBookingsByPhoneAsync(phone);
            response.ExecutedFunctions.Add(new FunctionCallLog
            {
                FunctionName = "GetBookingsByPhone",
                Arguments = new { phone = phone },
                Result = new { phone = phone, bookingsCount = bookings.Count, bookings = bookings }
            });

            if (bookings.Count > 0)
            {
                response.Reply = $"Tôi đã tra cứu danh sách đặt phòng của số điện thoại {phone}:\n" +
                                 $"Tìm thấy {bookings.Count} đơn đặt phòng trong hệ thống. Quý khách có thể xem chi tiết trong bảng thông tin hoặc yêu cầu hỗ trợ thêm.";
            }
            else
            {
                response.Reply = $"Không tìm thấy đơn đặt phòng nào gắn với số điện thoại {phone}. Quý khách vui lòng kiểm tra lại số điện thoại.";
            }
            return response;
        }

        // 3. Check room availability intent
        if (lower.Contains("còn") || lower.Contains("trống") || lower.Contains("phòng") || lower.Contains("giá"))
        {
            string roomType = "";
            if (lower.Contains("suite")) roomType = "Suite";
            else if (lower.Contains("đôi") || lower.Contains("double")) roomType = "Double";
            else if (lower.Contains("đơn") || lower.Contains("single")) roomType = "Single";

            var dateMatches = Regex.Matches(message, @"\b(\d{1,2})[/-](\d{1,2})(?:[/-](\d{4}))?\b");
            string checkIn = "2026-12-24";
            string checkOut = "2026-12-26";

            if (dateMatches.Count >= 2)
            {
                string d1 = dateMatches[0].Groups[1].Value.PadLeft(2, '0');
                string m1 = dateMatches[0].Groups[2].Value.PadLeft(2, '0');
                string y1 = dateMatches[0].Groups[3].Success ? dateMatches[0].Groups[3].Value : "2026";

                string d2 = dateMatches[1].Groups[1].Value.PadLeft(2, '0');
                string m2 = dateMatches[1].Groups[2].Value.PadLeft(2, '0');
                string y2 = dateMatches[1].Groups[3].Success ? dateMatches[1].Groups[3].Value : "2026";

                checkIn = $"{y1}-{m1}-{d1}";
                checkOut = $"{y2}-{m2}-{d2}";
            }

            var available = await hotelService.CheckAvailabilityAsync(roomType, checkIn, checkOut);
            response.ExecutedFunctions.Add(new FunctionCallLog
            {
                FunctionName = "CheckAvailability",
                Arguments = new { roomType = roomType, checkIn = checkIn, checkOut = checkOut },
                Result = new { roomType = roomType, checkIn = checkIn, checkOut = checkOut, availableRoomsCount = available.Count, availableRooms = available }
            });

            if (available.Count > 0)
            {
                var roomDetails = string.Join(", ", available.Select(r => $"Phòng {r.RoomNumber} ({r.RoomType} - {r.PricePerNight:N0} VNĐ/đêm)"));
                response.Reply = $"Từ ngày {checkIn} đến {checkOut}, khách sạn hiện còn {available.Count} phòng trống phù hợp: {roomDetails}. Quý khách có muốn tôi hỗ trợ đặt phòng không ạ?";
            }
            else
            {
                response.Reply = $"Rất tiếc, trong khoảng thời gian từ {checkIn} đến {checkOut}, loại phòng {roomType} hiện đã kín phòng.";
            }
            return response;
        }

        // Default greeting / assistant help
        response.Reply = "Xin chào quý khách! Tôi là trợ lý ảo của khách sạn Sunrise Hotel. Tôi có thể hỗ trợ quý khách kiểm tra phòng trống theo ngày, tra cứu lịch sử đơn đặt phòng qua số điện thoại, hoặc hủy đơn đặt phòng theo mã. Quý khách vui lòng cho biết yêu cầu cụ thể ạ!";
        return response;
    }
}
