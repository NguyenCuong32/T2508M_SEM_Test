using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using baithi.Models;
using baithi.Repositories;

namespace baithi.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IBookingRepository _bookingRepo;
    private readonly ILogger<GeminiService> _logger;

    private const string BaseApiUrl = "https://generativelanguage.googleapis.com/v1beta/models";

    // Danh sách các mô hình ưu tiên, tự động fallback nếu model bận hoặc bị giới hạn
    private static readonly string[] DefaultModelPriority = new[]
    {
        "gemini-3.5-flash-lite",
        "gemini-3.5-flash",
        "gemini-2.5-flash-lite",
        "gemini-3.8-flash",
        "gemini-flash-latest",
        "gemini-2.5-flash"
    };

    public GeminiService(
        HttpClient httpClient,
        IConfiguration configuration,
        IBookingRepository bookingRepo,
        ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _bookingRepo = bookingRepo;
        _logger = logger;
    }

    private string GetApiKey()
    {
        var apiKey = _configuration["GEMINI_API_KEY"]
            ?? _configuration["Gemini:ApiKey"]
            ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY")
            ?? Environment.GetEnvironmentVariable("Gemini__ApiKey");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Gemini API Key chưa được cấu hình! Vui lòng cấu hình qua User Secrets (dotnet user-secrets set \"Gemini:ApiKey\" \"<YOUR_API_KEY>\") hoặc biến môi trường GEMINI_API_KEY.");
        }

        return apiKey;
    }

    private List<string> GetCandidateModels()
    {
        var customModel = _configuration["Gemini:Model"] ?? Environment.GetEnvironmentVariable("GEMINI_MODEL");
        var list = new List<string>();
        if (!string.IsNullOrWhiteSpace(customModel))
        {
            list.Add(customModel);
        }
        foreach (var m in DefaultModelPriority)
        {
            if (!list.Contains(m)) list.Add(m);
        }
        return list;
    }

    public async Task<string> AskHotelAsync(string question)
    {
        var apiKey = GetApiKey();
        var models = GetCandidateModels();

        var systemPrompt = @"Bạn là nhân viên lễ tân lịch sự, chu đáo và chuyên nghiệp của khách sạn Sunrise Hotel.
Thông tin khách sạn Sunrise Hotel:
- Địa chỉ: 25 Trần Phú, Nha Trang.
- Giờ nhận phòng (check-in): từ 14:00.
- Giờ trả phòng (check-out): trước 12:00.
- Ăn sáng miễn phí: từ 6:30 đến 9:30 mỗi ngày.
- Miễn phí đón sân bay cho khách đặt hạng phòng Suite.
- Hồ bơi ngoài trời nằm ở tầng 5.
- Bảng giá các hạng phòng:
  + Phòng Single (Đơn): 600.000 VND / đêm (Tối đa 1 khách).
  + Phòng Double (Đôi): 950.000 VND / đêm (Tối đa 2 khách).
  + Phòng Suite (VIP): 2.200.000 VND / đêm (Tối đa 4 khách).

Quy tắc ứng xử:
1. Luôn chào đón khách niềm nở, lịch sự, đóng vai lễ tân khách sạn Sunrise Hotel.
2. CHỈ trả lời các câu hỏi liên quan đến Sunrise Hotel, phòng ốc, tiện ích và dịch vụ của khách sạn.
3. Nếu khách hỏi về các vấn đề ngoài khách sạn (như toán học, thời sự, lập trình, xã hội, chủ đề khác), hãy từ chối một cách khéo léo, lịch sự và hướng dẫn khách quay lại các thông tin của Sunrise Hotel.";

        var requestBody = new
        {
            system_instruction = new
            {
                parts = new object[]
                {
                    new { text = systemPrompt }
                }
            },
            contents = new object[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new { text = question }
                    }
                }
            }
        };

        var responseJson = await ExecuteWithCandidateModelsAsync(models, apiKey, requestBody);
        return ExtractTextFromResponse(responseJson);
    }

    public async Task<ParsedBookingDto> ParseBookingAsync(string text)
    {
        var apiKey = GetApiKey();
        var models = GetCandidateModels();

        var prompt = $@"Bạn là trợ lý trích xuất dữ liệu đặt phòng tự động cho khách sạn Sunrise Hotel.
Nhiệm vụ: Trích xuất các trường thông tin đặt phòng từ văn bản của khách hàng thành JSON.

Quy tắc bắt buộc:
1. Quy đổi loại phòng về đúng 1 trong 3 giá trị:
   - 'phòng đơn' hoặc tương tự -> 'Single'
   - 'phòng đôi' hoặc tương tự -> 'Double'
   - 'phòng Suite' hoặc 'phòng VIP' hoặc tương tự -> 'Suite'
2. Định dạng ngày nhận phòng (checkIn) và ngày trả phòng (checkOut) BẮT BUỘC là 'yyyy-MM-dd'.
3. guestName: Họ tên khách hàng.
4. phone: Số điện thoại của khách hàng.
5. guests: Số lượng khách (kiểu số nguyên).

Văn bản từ khách hàng:
""{text}""";

        var requestBody = new
        {
            contents = new object[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new { text = prompt }
                    }
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
                        guestName = new { type = "STRING", description = "Họ tên khách" },
                        phone = new { type = "STRING", description = "Số điện thoại khách" },
                        roomType = new { type = "STRING", @enum = new[] { "Single", "Double", "Suite" }, description = "Loại phòng chuẩn: Single, Double, hoặc Suite" },
                        checkIn = new { type = "STRING", description = "Ngày nhận phòng định dạng yyyy-MM-dd" },
                        checkOut = new { type = "STRING", description = "Ngày trả phòng định dạng yyyy-MM-dd" },
                        guests = new { type = "INTEGER", description = "Số khách tối đa" }
                    },
                    required = new[] { "guestName", "phone", "roomType", "checkIn", "checkOut", "guests" }
                }
            }
        };

        var responseJson = await ExecuteWithCandidateModelsAsync(models, apiKey, requestBody);
        var jsonText = ExtractTextFromResponse(responseJson);

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<ParsedBookingDto>(jsonText, options);
            if (result == null)
            {
                throw new JsonException("Không thể phân tích dữ liệu JSON trả về từ AI.");
            }
            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Lỗi phân tích JSON từ phản hồi AI: {RawResponse}", jsonText);
            throw new ApplicationException($"Lỗi phân tích JSON từ AI: {ex.Message}. Phản hồi thô: {jsonText}", ex);
        }
    }

    public async Task<BookingChatResponse> BookingAssistantChatAsync(string message)
    {
        var apiKey = GetApiKey();
        var models = GetCandidateModels();

        var systemPrompt = @"Bạn là trợ lý ảo AI thông minh và lịch thiệp của khách sạn Sunrise Hotel (25 Trần Phú, Nha Trang).
Bạn có nhiệm vụ giải đáp và thực hiện các yêu cầu đặt phòng của khách hàng thông qua việc sử dụng các công cụ (tools) được cung cấp:
1. Khi khách hỏi kiểm tra phòng trống hoặc hỏi giá phòng theo ngày, hãy gọi hàm CheckAvailability.
2. Khi khách yêu cầu tra cứu đơn đặt phòng theo số điện thoại, hãy gọi hàm GetBookingsByPhone.
3. Khi khách yêu cầu hủy đơn đặt phòng theo mã booking ID, hãy gọi hàm CancelBooking.

Hãy trả lời tự nhiên, thân thiện và đầy đủ thông tin cho khách dựa trên kết quả trả về từ công cụ.";

        var toolsDeclaration = new object[]
        {
            new
            {
                functionDeclarations = new object[]
                {
                    new
                    {
                        name = "CheckAvailability",
                        description = "Truy vấn DB và trả về danh sách các phòng còn trống thuộc loại phòng đó trong khoảng ngày yêu cầu kèm mức giá tại Sunrise Hotel.",
                        parameters = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                roomType = new
                                {
                                    type = "STRING",
                                    @enum = new[] { "Single", "Double", "Suite" },
                                    description = "Loại phòng cần kiểm tra (Single, Double, hoặc Suite)"
                                },
                                checkIn = new
                                {
                                    type = "STRING",
                                    description = "Ngày nhận phòng định dạng chuẩn yyyy-MM-dd"
                                },
                                checkOut = new
                                {
                                    type = "STRING",
                                    description = "Ngày trả phòng định dạng chuẩn yyyy-MM-dd"
                                }
                            },
                            required = new[] { "roomType", "checkIn", "checkOut" }
                        }
                    },
                    new
                    {
                        name = "GetBookingsByPhone",
                        description = "Truy vấn DB và trả về danh sách các đơn đặt phòng của khách hàng tại Sunrise Hotel theo số điện thoại.",
                        parameters = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                phone = new
                                {
                                    type = "STRING",
                                    description = "Số điện thoại của khách hàng cần tra cứu đơn đặt phòng"
                                }
                            },
                            required = new[] { "phone" }
                        }
                    },
                    new
                    {
                        name = "CancelBooking",
                        description = "Tìm đơn đặt phòng theo bookingId và cập nhật trạng thái Status thành Cancelled.",
                        parameters = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                bookingId = new
                                {
                                    type = "INTEGER",
                                    description = "Mã số định danh đơn đặt phòng (BookingId) cần hủy"
                                }
                            },
                            required = new[] { "bookingId" }
                        }
                    }
                }
            }
        };

        var initialContents = new List<object>
        {
            new
            {
                role = "user",
                parts = new object[]
                {
                    new { text = message }
                }
            }
        };

        var requestBody = new
        {
            system_instruction = new
            {
                parts = new object[]
                {
                    new { text = systemPrompt }
                }
            },
            contents = initialContents,
            tools = toolsDeclaration
        };

        var responseJson = await ExecuteWithCandidateModelsAsync(models, apiKey, requestBody);
        var executedFunctions = new List<string>();

        // Check for function calls in the response
        var rootNode = JsonNode.Parse(responseJson);
        var partsArray = rootNode?["candidates"]?[0]?["content"]?["parts"] as JsonArray;

        if (partsArray != null)
        {
            JsonObject? functionCallObj = null;
            foreach (var part in partsArray)
            {
                if (part?["functionCall"] is JsonObject fc)
                {
                    functionCallObj = fc;
                    break;
                }
            }

            if (functionCallObj != null)
            {
                var functionName = functionCallObj["name"]?.GetValue<string>() ?? string.Empty;
                var functionArgs = functionCallObj["args"] as JsonObject ?? new JsonObject();
                executedFunctions.Add(functionName);

                _logger.LogInformation("Gemini requested function call: {FunctionName} with args: {Args}",
                    functionName, functionArgs.ToJsonString());

                object functionResult = await ExecuteFunctionAsync(functionName, functionArgs);
                var modelContent = rootNode?["candidates"]?[0]?["content"];

                // Build follow-up request with function execution result
                var multiTurnContents = new List<object>
                {
                    new
                    {
                        role = "user",
                        parts = new object[]
                        {
                            new { text = message }
                        }
                    },
                    modelContent!,
                    new
                    {
                        role = "user",
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
                    }
                };

                var followUpRequestBody = new
                {
                    system_instruction = new
                    {
                        parts = new object[]
                        {
                            new { text = systemPrompt }
                        }
                    },
                    contents = multiTurnContents,
                    tools = toolsDeclaration
                };

                var secondResponseJson = await ExecuteWithCandidateModelsAsync(models, apiKey, followUpRequestBody);
                var finalAnswer = ExtractTextFromResponse(secondResponseJson);

                return new BookingChatResponse
                {
                    Answer = finalAnswer,
                    ExecutedFunctions = executedFunctions
                };
            }
        }

        // No function call was triggered; return text directly
        var directAnswer = ExtractTextFromResponse(responseJson);
        return new BookingChatResponse
        {
            Answer = directAnswer,
            ExecutedFunctions = executedFunctions
        };
    }

    private async Task<object> ExecuteFunctionAsync(string functionName, JsonObject args)
    {
        switch (functionName)
        {
            case "CheckAvailability":
            {
                var roomType = args["roomType"]?.GetValue<string>() ?? "Double";
                var checkInStr = args["checkIn"]?.GetValue<string>() ?? string.Empty;
                var checkOutStr = args["checkOut"]?.GetValue<string>() ?? string.Empty;

                if (!DateOnly.TryParse(checkInStr, out var checkIn) || !DateOnly.TryParse(checkOutStr, out var checkOut))
                {
                    return new
                    {
                        success = false,
                        error = "Ngày check-in hoặc check-out không đúng định dạng yyyy-MM-dd."
                    };
                }

                if (checkOut <= checkIn)
                {
                    return new
                    {
                        success = false,
                        error = "Ngày trả phòng (checkOut) phải sau ngày nhận phòng (checkIn)."
                    };
                }

                var availableRooms = await _bookingRepo.GetAvailableRoomsAsync(roomType, checkIn, checkOut);
                return new
                {
                    success = true,
                    roomType,
                    checkIn = checkIn.ToString("yyyy-MM-dd"),
                    checkOut = checkOut.ToString("yyyy-MM-dd"),
                    availableRoomsCount = availableRooms.Count,
                    rooms = availableRooms.Select(r => new
                    {
                        roomId = r.RoomId,
                        roomNumber = r.RoomNumber,
                        roomType = r.RoomType,
                        pricePerNight = r.PricePerNight,
                        pricePerNightFormatted = $"{r.PricePerNight:N0} VND",
                        maxGuests = r.MaxGuests
                    }).ToList()
                };
            }

            case "GetBookingsByPhone":
            {
                var phone = args["phone"]?.GetValue<string>() ?? string.Empty;
                var bookings = await _bookingRepo.GetBookingsByPhoneAsync(phone);

                return new
                {
                    success = true,
                    phone,
                    bookingsCount = bookings.Count,
                    bookings = bookings.Select(b => new
                    {
                        bookingId = b.BookingId,
                        roomNumber = b.Room?.RoomNumber ?? "N/A",
                        roomType = b.Room?.RoomType ?? "N/A",
                        guestName = b.GuestName,
                        checkIn = b.CheckIn.ToString("yyyy-MM-dd"),
                        checkOut = b.CheckOut.ToString("yyyy-MM-dd"),
                        totalPrice = b.TotalPrice,
                        totalPriceFormatted = $"{b.TotalPrice:N0} VND",
                        status = b.Status
                    }).ToList()
                };
            }

            case "CancelBooking":
            {
                int bookingId = 0;
                if (args["bookingId"] != null)
                {
                    int.TryParse(args["bookingId"]!.ToString(), out bookingId);
                }

                if (bookingId <= 0)
                {
                    return new { success = false, message = "Mã bookingId không hợp lệ." };
                }

                var cancelled = await _bookingRepo.CancelBookingAsync(bookingId);
                if (cancelled == null)
                {
                    return new
                    {
                        success = false,
                        bookingId,
                        message = $"Không tìm thấy đơn đặt phòng với mã BookingId={bookingId} để hủy."
                    };
                }

                return new
                {
                    success = true,
                    bookingId = cancelled.BookingId,
                    guestName = cancelled.GuestName,
                    roomNumber = cancelled.Room?.RoomNumber,
                    status = cancelled.Status,
                    message = $"Đã hủy thành công đơn đặt phòng số {cancelled.BookingId} cho khách hàng {cancelled.GuestName}."
                };
            }

            default:
                return new { success = false, message = $"Hàm {functionName} không được hỗ trợ." };
        }
    }

    private async Task<string> ExecuteWithCandidateModelsAsync(List<string> candidateModels, string apiKey, object payload)
    {
        Exception? lastException = null;

        foreach (var model in candidateModels)
        {
            try
            {
                var url = $"{BaseApiUrl}/{model}:generateContent?key={apiKey}";
                return await SendGeminiRequestAsync(url, payload);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound || 
                                                  ex.StatusCode == HttpStatusCode.ServiceUnavailable || 
                                                  ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning("Mô hình {Model} trả về {StatusCode}. Chuyển sang mô hình tiếp theo trong danh sách...", model, ex.StatusCode);
                lastException = ex;
                continue;
            }
        }

        throw lastException ?? new InvalidOperationException("Không thể gọi tới các mô hình Gemini trong danh sách.");
    }

    private async Task<string> SendGeminiRequestAsync(string url, object payload)
    {
        var jsonContent = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new HttpRequestException(
                    "Mô hình Gemini đang bị giới hạn tần suất yêu cầu (HTTP 429 Too Many Requests). Vui lòng thử lại sau vài giây.",
                    null, HttpStatusCode.TooManyRequests);
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API error (Status {StatusCode}): {ErrorBody}",
                    response.StatusCode, errorBody);

                if (response.StatusCode == HttpStatusCode.BadRequest && errorBody.Contains("API_KEY_INVALID"))
                {
                    throw new HttpRequestException(
                        "Gemini API Key không hợp lệ. Vui lòng kiểm tra lại khóa API của bạn trong User Secrets hoặc Biến môi trường.",
                        null, HttpStatusCode.BadRequest);
                }

                throw new HttpRequestException(
                    $"Gemini API trả về lỗi {response.StatusCode}: {errorBody}", null, response.StatusCode);
            }

            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi kết nối khi gửi yêu cầu tới Gemini API.");
            throw new ApplicationException($"Lỗi kết nối tới dịch vụ Google Gemini: {ex.Message}", ex);
        }
    }

    private static string ExtractTextFromResponse(string responseJson)
    {
        try
        {
            var root = JsonNode.Parse(responseJson);
            var candidates = root?["candidates"] as JsonArray;
            if (candidates == null || candidates.Count == 0)
            {
                return "Không nhận được phản hồi từ AI.";
            }

            var parts = candidates[0]?["content"]?["parts"] as JsonArray;
            if (parts != null)
            {
                var sb = new StringBuilder();
                foreach (var part in parts)
                {
                    var text = part?["text"]?.GetValue<string>();
                    if (!string.IsNullOrEmpty(text))
                    {
                        sb.Append(text);
                    }
                }

                if (sb.Length > 0)
                {
                    return sb.ToString().Trim();
                }
            }

            return "Không có nội dung văn bản trong phản hồi từ AI.";
        }
        catch (Exception ex)
        {
            throw new JsonException($"Lỗi bóc tách nội dung phản hồi từ Gemini: {ex.Message}");
        }
    }
}
