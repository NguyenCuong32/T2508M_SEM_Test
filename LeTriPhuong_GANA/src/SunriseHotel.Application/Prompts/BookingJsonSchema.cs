namespace SunriseHotel.Application.Prompts;

/// <summary>Response schema (OpenAPI subset) used for Gemini structured output in requirement 2.</summary>
public static class BookingJsonSchema
{
    public const string ParsedBooking = """
        {
          "type": "OBJECT",
          "properties": {
            "guestName": { "type": "STRING", "description": "Guest full name" },
            "phone":     { "type": "STRING", "description": "Phone number, digits only" },
            "roomType":  { "type": "STRING", "enum": ["Single", "Double", "Suite"] },
            "checkIn":   { "type": "STRING", "description": "Check-in date, yyyy-MM-dd" },
            "checkOut":  { "type": "STRING", "description": "Check-out date, yyyy-MM-dd" },
            "guests":    { "type": "INTEGER", "description": "Number of guests" }
          },
          "required": ["guestName", "phone", "roomType", "checkIn", "checkOut", "guests"],
          "propertyOrdering": ["guestName", "phone", "roomType", "checkIn", "checkOut", "guests"]
        }
        """;
}
