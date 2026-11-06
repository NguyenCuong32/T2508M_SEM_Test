using System.Text.Json;
using System.Text.Json.Nodes;
using SunriseHotel.Application.Abstractions.Ai;
using SunriseHotel.Application.Features.Bookings;

namespace SunriseHotel.Application.Features.Assistant.Functions;

public sealed class CancelBookingFunction(IBookingService bookingService) : IAssistantFunction
{
    public string Name => "CancelBooking";

    public AiFunctionDeclaration Declaration => new(
        Name,
        "Cancel a booking: change its status to Cancelled.",
        JsonNode.Parse("""
            {
              "type": "object",
              "properties": {
                "bookingId": { "type": "integer", "description": "Booking number (BookingId)" }
              },
              "required": ["bookingId"]
            }
            """)!);

    public async Task<object> ExecuteAsync(JsonElement arguments, CancellationToken ct = default)
    {
        var booking = await bookingService.CancelBookingAsync(FunctionArguments.GetInt(arguments, "bookingId"), ct);
        return new { success = true, booking };
    }
}
