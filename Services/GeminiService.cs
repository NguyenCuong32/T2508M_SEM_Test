using Google.GenAI;
using Google.GenAI.Types;
using SUNRISEHOTEL.Data;
using SUNRISEHOTEL.DTOs;
using System.Text.Json;

namespace SUNRISEHOTEL.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly Client _client;
        private readonly IConfiguration _configuration;

        public GeminiService(
            Client client,
            IConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;
        }

        public async Task<string> AskAsync(
            string prompt,
            string systemInstruction)
        {
            var model = _configuration["Gemini:Model"]
                        ?? "gemini-3.8-flash";

            var config = new GenerateContentConfig
            {
                SystemInstruction = new Content
                {
                    Parts = new List<Part>
                {
                    new Part { Text = systemInstruction }
                }
                },
                Temperature = 0.2f,
                MaxOutputTokens = 1024
            };

            var response = await _client.Models.GenerateContentAsync(
                model: model,
                contents: prompt,
                config: config
            );

            return response.Text ?? string.Empty;
        }

        public async Task<BookingRequestDTO> ParseBookingAsync(string text)
        {
            var model = _configuration["Gemini:Model"]
                        ?? "gemini-3.8-flash";

            var config = new GenerateContentConfig
            {
                SystemInstruction = new Content
                {
                    Parts = new List<Part>
            {
                new Part
                {
                    Text = """
                    You are a hotel booking information extractor.

                    Extract booking information from the user's message.

                    Rules:
                    - Return only structured JSON.
                    - Do not invent missing information.
                    - Convert room names:
                      phòng đơn = Single
                      phòng đôi = Double
                      phòng Suite = Suite
                    - Convert dates to YYYY-MM-DD.
                    - If a field is missing or ambiguous,
                      do not guess its value.
                    - Guests must be an integer.
                    """
                }
            }
                },
                ResponseMimeType = "application/json",
                ResponseJsonSchema = BookingSchema.Schema,
                Temperature = 0
            };

            var response = await _client.Models.GenerateContentAsync(
                model: model,
                contents: text,
                config: config
            );

            var json = response.Text;

            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException(
                    "Gemini returned an empty response."
                );

            return JsonSerializer.Deserialize<BookingRequestDTO>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? throw new InvalidOperationException(
                "Cannot parse booking information."
            );
        }
    }
}
