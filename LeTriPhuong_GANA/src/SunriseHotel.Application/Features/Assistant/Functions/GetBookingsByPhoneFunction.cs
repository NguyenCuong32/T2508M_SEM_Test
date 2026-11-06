using System.Text.Json;
using System.Text.Json.Nodes;
using SunriseHotel.Application.Abstractions.Ai;
using SunriseHotel.Application.Features.Bookings;

namespace SunriseHotel.Application.Features.Assistant.Functions;

public sealed class GetBookingsByPhoneFunction(IBookingService bookingService) : IAssistantFunction
{
    public string Name => "GetBookingsByPhone";

    public AiFunctionDeclaration Declaration => new(
        Name,
        "Return all bookings (confirmed and cancelled) of a guest, looked up by phone number.",
        JsonNode.Parse("""
            {
              "type": "object",
              "properties": {
                "phone": { "type": "string", "description": "Guest phone number, e.g. 0901234567" }
              },
              "required": ["phone"]
            }
            """)!);

    public async Task<object> ExecuteAsync(JsonElement arguments, CancellationToken ct = default)
    {
        var bookings = await bookingService.GetBookingsByPhoneAsync(FunctionArguments.GetString(arguments, "phone"), ct);
        return new { count = bookings.Count, bookings };
    }
}
