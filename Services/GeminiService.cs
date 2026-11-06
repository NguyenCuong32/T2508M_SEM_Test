using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SunriseHotel.DTOs;

namespace SunriseHotel.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiService> _logger;

    public const string HotelSystemInstruction = """
        You are a polite, professional receptionist at Sunrise Hotel.
        Only answer questions related to Sunrise Hotel based strictly on the following hotel information:
        - Hotel Name: Sunrise Hotel
        - Address: 25 Tran Phu, Nha Trang
        - Check-in time: from 14:00 (2:00 PM)
        - Check-out time: before 12:00 (12:00 PM)
        - Complimentary/Free breakfast: served from 06:30 to 09:30
        - Free airport pick-up: exclusively provided for Suite rooms
        - Swimming pool: located on the 5th floor
        - Room Types & Pricing:
          * Single: 600,000 VND / night, max 1 guest
          * Double: 950,000 VND / night, max 2 guests
          * Suite: 2,200,000 VND / night, max 4 guests

        Guidelines:
        1. Always maintain a courteous, welcoming, and helpful receptionist tone.
        2. Answer in the same language as the customer's question (e.g. Vietnamese or English).
        3. Strictly decline questions that are not related to Sunrise Hotel, its services, or hospitality inquiries, politely explaining that you can only assist with Sunrise Hotel inquiries.
        """;

    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    private string GetApiKey()
    {
        var apiKey = _configuration["Gemini:ApiKey"] 
                     ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Gemini API Key is not configured. Please set 'Gemini:ApiKey' in appsettings/user secrets or environment variable 'GEMINI_API_KEY'.");
        }
        return apiKey;
    }

    private string GetModel()
    {
        return _configuration["Gemini:Model"] ?? "gemini-3.5-flash";
    }

    public async Task<string> AskHotelAsync(string question, CancellationToken cancellationToken = default)
    {
        var apiKey = GetApiKey();
        var model = GetModel();
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var payload = new
        {
            system_instruction = new
            {
                parts = new[]
                {
                    new { text = HotelSystemInstruction }
                }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = question }
                    }
                }
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, jsonContent, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API call failed with status {StatusCode}: {Response}", response.StatusCode, responseString);
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    throw new HttpRequestException("Gemini API quota exceeded (HTTP 429). Please try again later.");
                }
                throw new HttpRequestException($"Gemini API error ({response.StatusCode}): {responseString}");
            }

            var jsonNode = JsonNode.Parse(responseString);
            var answer = jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

            return answer ?? "Xin lỗi, hiện tại tôi không thể xử lý câu trả lời. Quý khách vui lòng thử lại sau.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while communicating with Gemini API in AskHotelAsync");
            throw;
        }
    }

    public async Task<ParsedBookingData?> ParseBookingAsync(string text, CancellationToken cancellationToken = default)
    {
        var apiKey = GetApiKey();
        var model = GetModel();
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var systemInstruction = """
            You are a hotel booking data extraction system.
            Extract booking details from natural language text into a JSON object matching this exact schema:
            {
              "guestName": "Full name of the guest",
              "phone": "Phone number",
              "roomType": "Single" | "Double" | "Suite",
              "checkIn": "YYYY-MM-DD",
              "checkOut": "YYYY-MM-DD",
              "guests": integer
            }

            Rules:
            1. roomType must strictly be normalized to one of: "Single", "Double", "Suite".
               - "phòng đơn", "single room" -> "Single"
               - "phòng đôi", "double room" -> "Double"
               - "phòng suite", "suite room" -> "Suite"
            2. Convert all dates to standard "YYYY-MM-DD" format (e.g., 20/11/2026 -> 2026-11-20).
            3. Return ONLY valid JSON. Do not include markdown codeblocks or any additional explanation.
            """;

        var payload = new
        {
            system_instruction = new
            {
                parts = new[]
                {
                    new { text = systemInstruction }
                }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text }
                    }
                }
            },
            generationConfig = new
            {
                response_mime_type = "application/json"
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, jsonContent, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API call failed with status {StatusCode}: {Response}", response.StatusCode, responseString);
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    throw new HttpRequestException("Gemini API quota exceeded (HTTP 429). Please try again later.");
                }
                throw new HttpRequestException($"Gemini API error ({response.StatusCode}): {responseString}");
            }

            var jsonNode = JsonNode.Parse(responseString);
            var rawJson = jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

            if (string.IsNullOrWhiteSpace(rawJson))
            {
                _logger.LogWarning("Gemini returned empty content when parsing booking text.");
                return null;
            }

            // Remove markdown code fences if model enclosed them
            rawJson = rawJson.Trim();
            if (rawJson.StartsWith("```json"))
            {
                rawJson = rawJson.Substring(7);
            }
            if (rawJson.StartsWith("```"))
            {
                rawJson = rawJson.Substring(3);
            }
            if (rawJson.EndsWith("```"))
            {
                rawJson = rawJson.Substring(0, rawJson.Length - 3);
            }
            rawJson = rawJson.Trim();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<ParsedBookingData>(rawJson, options);
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Failed to deserialize JSON returned by Gemini: {Text}", text);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while communicating with Gemini API in ParseBookingAsync");
            throw;
        }
    }

    public async Task<string> ChatAssistantAsync(
        string userMessage,
        Func<string, JsonElement, Task<object>> executeToolFunc,
        CancellationToken cancellationToken = default)
    {
        var apiKey = GetApiKey();
        var model = GetModel();
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var systemInstruction = $"""
            {HotelSystemInstruction}

            You are also a Sunrise Hotel Assistant equipped with database tools.
            When users ask about room availability, looking up their bookings, or cancelling a booking, ALWAYS call the appropriate function tool to query or perform changes in the real database.
            Never invent, hallucinate, or assume availability or booking status without calling the tools.
            If user mentions relative dates like "24/12 đến 26/12", use year 2026 or the current calendar year. Format dates as YYYY-MM-DD.
            After receiving the tool execution result, synthesize a clear, helpful, and courteous answer for the user.
            """;

        // Declare the 3 functions as required by Requirement 3
        var tools = new[]
        {
            new
            {
                function_declarations = new object[]
                {
                    new
                    {
                        name = "CheckAvailability",
                        description = "Return available rooms and price.",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                roomType = new { type = "string", description = "Type of room: Single, Double, or Suite" },
                                checkIn = new { type = "string", description = "Check-in date formatted as YYYY-MM-DD" },
                                checkOut = new { type = "string", description = "Check-out date formatted as YYYY-MM-DD" }
                            },
                            required = new[] { "roomType", "checkIn", "checkOut" }
                        }
                    },
                    new
                    {
                        name = "GetBookingsByPhone",
                        description = "Return the bookings of a guest.",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                phone = new { type = "string", description = "Phone number of the guest" }
                            },
                            required = new[] { "phone" }
                        }
                    },
                    new
                    {
                        name = "CancelBooking",
                        description = "Change booking status to Cancelled.",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                bookingId = new { type = "integer", description = "The ID of the booking to cancel" }
                            },
                            required = new[] { "bookingId" }
                        }
                    }
                }
            }
        };

        var contents = new List<object>
        {
            new
            {
                role = "user",
                parts = new object[]
                {
                    new { text = userMessage }
                }
            }
        };

        var firstPayload = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = systemInstruction } }
            },
            tools,
            contents
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(firstPayload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, jsonContent, cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini API call failed in ChatAssistantAsync: {StatusCode} {Response}", response.StatusCode, responseString);
            throw new HttpRequestException($"Gemini API error ({response.StatusCode}): {responseString}");
        }

        using var doc = JsonDocument.Parse(responseString);
        var root = doc.RootElement;
        var candidate = root.GetProperty("candidates")[0];
        var content = candidate.GetProperty("content");
        var parts = content.GetProperty("parts");

        // Check if Gemini invoked a function call
        JsonElement? functionCallPart = null;
        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("functionCall", out var fc))
            {
                functionCallPart = fc;
                break;
            }
        }

        // If no function call, return direct text response
        if (!functionCallPart.HasValue)
        {
            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var textElem))
                {
                    return textElem.GetString() ?? string.Empty;
                }
            }
            return "Xin chào, tôi có thể hỗ trợ gì cho quý khách về đặt phòng tại Sunrise Hotel?";
        }

        // Extract function call details
        var fnName = functionCallPart.Value.GetProperty("name").GetString() ?? string.Empty;
        var fnArgs = functionCallPart.Value.GetProperty("args");

        _logger.LogInformation("Gemini requested function call: {FnName} with args: {Args}", fnName, fnArgs.GetRawText());

        // Execute C# logic against database
        var toolResult = await executeToolFunc(fnName, fnArgs);

        // Turn 2: Send function response back to Gemini (Gemini v1beta uses role: "user")
        var modelTurnContent = JsonSerializer.Deserialize<object>(content.GetRawText())!;
        var functionResponseTurn = new
        {
            role = "user",
            parts = new object[]
            {
                new
                {
                    functionResponse = new
                    {
                        name = fnName,
                        response = toolResult
                    }
                }
            }
        };

        var updatedContents = new List<object>
        {
            contents[0],
            modelTurnContent,
            functionResponseTurn
        };

        var secondPayload = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = systemInstruction } }
            },
            tools,
            contents = updatedContents
        };

        var secondJsonContent = new StringContent(JsonSerializer.Serialize(secondPayload), Encoding.UTF8, "application/json");
        var secondResponse = await _httpClient.PostAsync(url, secondJsonContent, cancellationToken);
        var secondResponseString = await secondResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!secondResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini second turn failed in ChatAssistantAsync: {StatusCode} {Response}", secondResponse.StatusCode, secondResponseString);
            throw new HttpRequestException($"Gemini API error ({secondResponse.StatusCode}): {secondResponseString}");
        }

        using var secondDoc = JsonDocument.Parse(secondResponseString);
        var secondCandidate = secondDoc.RootElement.GetProperty("candidates")[0];
        var secondParts = secondCandidate.GetProperty("content").GetProperty("parts");

        foreach (var part in secondParts.EnumerateArray())
        {
            if (part.TryGetProperty("text", out var textElem))
            {
                return textElem.GetString() ?? string.Empty;
            }
        }

        return "Yêu cầu của bạn đã được thực hiện xong.";
    }
}
