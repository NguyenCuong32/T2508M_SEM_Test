using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using SunriseHotel.Models;
using SunriseHotel.Repositories;
namespace SunriseHotel.Services;

public class GeminiService(HttpClient http, IConfiguration config, IBookingRepository repo, IRoomRepository rooms) : IGeminiService
{
    public const string HotelPrompt = "Bạn là lễ tân lịch sự của Sunrise Hotel. Chỉ trả lời câu hỏi về Sunrise Hotel, từ chối câu hỏi không liên quan. Thông tin: địa chỉ 25 Trần Phú, Nha Trang; nhận phòng từ 14:00, trả phòng trước 12:00; ăn sáng miễn phí 6:30-9:30; đón sân bay miễn phí cho phòng Suite; hồ bơi tầng 5. Không bịa thông tin.";
    public const string ParsePrompt = "Trích xuất yêu cầu đặt phòng thành JSON theo schema. Single là phòng đơn, Double là phòng đôi, Suite là phòng suite. Ngày định dạng YYYY-MM-DD. Không suy đoán thông tin thiếu; dùng chuỗi rỗng hoặc 0 nếu thiếu.";
    public const string AssistantPrompt = HotelPrompt + " Bạn có thể kiểm tra phòng, tìm booking theo số điện thoại và hủy booking bằng các công cụ được cung cấp. Không khẳng định hành động thành công nếu công cụ không xác nhận. Nếu thiếu thông tin hãy hỏi lại. Ngày hiện tại: ";
    static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);
    static object Schema(string description, object properties, string[] required) => new { type = "OBJECT", description, properties, required };
    static object S(string description) => new { type = "STRING", description };
    static object[] Declarations => [
     new {name="CheckAvailability",description="Tìm phòng trống và giá theo loại và khoảng ngày",parameters=Schema("Kiểm tra phòng",new {roomType=S("Single, Double hoặc Suite"),checkIn=S("YYYY-MM-DD"),checkOut=S("YYYY-MM-DD")},["roomType","checkIn","checkOut"])},
  new {name="GetBookingsByPhone",description="Tìm các booking theo số điện thoại",parameters=Schema("Tìm booking",new {phone=S("Số điện thoại")},["phone"])},
  new {name="CancelBooking",description="Hủy booking theo mã",parameters=Schema("Hủy booking",new {bookingId=new {type="INTEGER",description="Mã booking"}},["bookingId"])}
    ];
    async Task<JsonNode> Generate(object payload)
    {
        var key = config["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY"); if (string.IsNullOrWhiteSpace(key)) throw new GeminiException("Chưa cấu hình GEMINI_API_KEY.", 503);
        var model = config["Gemini:Model"] ?? "gemini-3.1-flash-lite";
        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(model)}:generateContent"); request.Headers.Add("x-goog-api-key", key); request.Content = JsonContent.Create(payload);
        using var response = await http.SendAsync(request); var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) { if (response.StatusCode == HttpStatusCode.TooManyRequests) throw new GeminiException("Gemini đang giới hạn số yêu cầu. Vui lòng thử lại sau.", 429); throw new GeminiException($"Gemini không xử lý được yêu cầu (HTTP {(int)response.StatusCode})."); }
        try { return JsonNode.Parse(body) ?? throw new JsonException(); } catch (JsonException) { throw new GeminiException("Gemini trả về phản hồi không hợp lệ."); }
    }
    static JsonArray Parts(JsonNode response) => response["candidates"]?[0]?["content"]?["parts"]?.AsArray() ?? throw new GeminiException("Gemini không trả về nội dung.");
    static string Text(JsonNode response) => string.Join("", Parts(response).Select(p => p?["text"]?.GetValue<string>() ?? "")) is var s && !string.IsNullOrWhiteSpace(s) ? s : throw new GeminiException("Gemini không trả về văn bản.");
    public async Task<string> Ask(string question) => Text(await Generate(new { systemInstruction = new { parts = new[] { new { text = HotelPrompt } } }, contents = new[] { new { role = "user", parts = new[] { new { text = question } } } } }));
    public async Task<BookingRequest> Parse(string text)
    {
        var schema = Schema("Thông tin đặt phòng", new { guestName = S("Họ tên"), phone = S("Số điện thoại"), roomType = S("Single, Double, Suite"), checkIn = S("YYYY-MM-DD"), checkOut = S("YYYY-MM-DD"), guests = new { type = "INTEGER" } }, ["guestName", "phone", "roomType", "checkIn", "checkOut", "guests"]);
        var response = await Generate(new { systemInstruction = new { parts = new[] { new { text = ParsePrompt } } }, contents = new[] { new { role = "user", parts = new[] { new { text } } } }, generationConfig = new { responseMimeType = "application/json", responseSchema = schema } });
        try
        {
            var parsed = JsonSerializer.Deserialize<ParsedBookingDto>(Text(response), JsonOpts) ?? throw new JsonException();
            static DateOnly ParseDate(string? value) => DateOnly.TryParseExact(value, new[] { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy" }, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var date) ? date : default;
            return new BookingRequest(parsed.GuestName ?? "", parsed.Phone ?? "", parsed.RoomType ?? "", ParseDate(parsed.CheckIn), ParseDate(parsed.CheckOut), parsed.Guests);
        }
        catch (JsonException) { throw new GeminiException("Gemini trả về thông tin đặt phòng không hợp lệ; vui lòng bổ sung đầy đủ họ tên, số điện thoại, loại phòng và ngày.", 422); }
    }
    public async Task<string> Chat(string message)
    {
        var contents = new JsonArray(new JsonObject { ["role"] = "user", ["parts"] = new JsonArray(new JsonObject { ["text"] = message }) });
        for (int i = 0; i < 5; i++)
        {
            var result = await Generate(new { systemInstruction = new { parts = new[] { new { text = AssistantPrompt + DateTime.UtcNow.ToString("yyyy-MM-dd") } } }, contents, tools = new[] { new { functionDeclarations = Declarations } } });
            var parts = Parts(result); var calls = parts.Where(p => p?["functionCall"] != null).ToList(); if (calls.Count == 0) return Text(result);
            contents.Add(new JsonObject { ["role"] = "model", ["parts"] = parts.DeepClone() }); var replies = new JsonArray();
            foreach (var part in calls)
            {
                var call = part!["functionCall"]!; string name = call["name"]?.GetValue<string>() ?? ""; var args = call["args"]; object value;
                try
                {
                    value = name switch
                    {
                        "CheckAvailability" => await Check(args),
                        "GetBookingsByPhone" => await repo.ByPhone(args?["phone"]?.GetValue<string>() ?? ""),
                        "CancelBooking" => new { cancelled = await repo.Cancel(args?["bookingId"]?.GetValue<int>() ?? 0) },
                        _ => new { error = "Unknown function" }
                    };
                }
                catch (FormatException) { value = new { error = "Ngày hoặc tham số không hợp lệ" }; }
                catch (ArgumentException) { value = new { error = "Ngày hoặc tham số không hợp lệ" }; }
                replies.Add(new JsonObject { ["functionResponse"] = new JsonObject { ["name"] = name, ["response"] = new JsonObject { ["result"] = JsonSerializer.SerializeToNode(value, JsonOpts) } } });
            }
            contents.Add(new JsonObject { ["role"] = "user", ["parts"] = replies });
        }
        throw new GeminiException("Gemini vượt quá số lượt gọi công cụ.");
    }
    async Task<object> Check(JsonNode? args) { var type = args?["roomType"]?.GetValue<string>() ?? ""; var start = DateOnly.ParseExact(args?["checkIn"]?.GetValue<string>() ?? "", "yyyy-MM-dd"); var end = DateOnly.ParseExact(args?["checkOut"]?.GetValue<string>() ?? "", "yyyy-MM-dd"); if (!ValidType(type) || end <= start) return new { error = "Loại phòng hoặc khoảng ngày không hợp lệ" }; return await rooms.Available(type, start, end); }
    public static bool ValidType(string? type) => type is "Single" or "Double" or "Suite";
}

