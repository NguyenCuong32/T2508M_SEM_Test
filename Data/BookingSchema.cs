using System.Text.Json.Nodes;

namespace SUNRISEHOTEL.Data
{
    public static class BookingSchema
    {
        public static JsonNode Schema => JsonNode.Parse("""
    {
      "type": "object",
      "properties": {
        "guestName": {
          "type": "string"
        },
        "phone": {
          "type": "string"
        },
        "roomType": {
          "type": "string",
          "enum": ["Single", "Double", "Suite"]
        },
        "checkIn": {
          "type": "string",
          "description": "Date in YYYY-MM-DD format"
        },
        "checkOut": {
          "type": "string",
          "description": "Date in YYYY-MM-DD format"
        },
        "guests": {
          "type": "integer"
        }
      },
      "required": [
        "guestName",
        "phone",
        "roomType",
        "checkIn",
        "checkOut",
        "guests"
      ],
      "additionalProperties": false
    }
    """)!;
    }
}
