using System.Text.Json;
using System.Text.Json.Nodes;
using SunriseHotel.Application.Abstractions.Ai;
using SunriseHotel.Application.Features.Bookings;

namespace SunriseHotel.Application.Features.Assistant.Functions;

public sealed class CheckAvailabilityFunction(IBookingService bookingService) : IAssistantFunction
{
    public string Name => "CheckAvailability";

    public AiFunctionDeclaration Declaration => new(
        Name,
        "Return the available rooms of a room type for a stay, with price per night and total price.",
        JsonNode.Parse("""
            {
              "type": "object",
              "properties": {
                "roomType": { "type": "string", "enum": ["Single", "Double", "Suite"], "description": "Room type" },
                "checkIn":  { "type": "string", "description": "Check-in date, format yyyy-MM-dd" },
                "checkOut": { "type": "string", "description": "Check-out date, format yyyy-MM-dd" }
              },
              "required": ["roomType", "checkIn", "checkOut"]
            }
            """)!);

    public async Task<object> ExecuteAsync(JsonElement arguments, CancellationToken ct = default) =>
        await bookingService.CheckAvailabilityAsync(
            FunctionArguments.GetRoomType(arguments, "roomType"),
            FunctionArguments.GetDate(arguments, "checkIn"),
            FunctionArguments.GetDate(arguments, "checkOut"),
            ct);
}
