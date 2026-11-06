using System.Text.Json;
using Microsoft.Extensions.Logging;
using SunriseHotel.Application.Abstractions.Ai;
using SunriseHotel.Application.Abstractions.Time;
using SunriseHotel.Application.Common.Exceptions;
using SunriseHotel.Application.Features.Bookings;
using SunriseHotel.Application.Prompts;

namespace SunriseHotel.Application.Features.BookingParsing;

public sealed record ParseBookingResponse(ParsedBookingDto Parsed, BookingDto Booking);

public interface IBookingParserService
{
    Task<ParseBookingResponse> ParseAndCreateAsync(string text, CancellationToken ct = default);
}

/// <summary>
/// Requirement 2: natural language -> Gemini JSON (structured output) -> C# object -> validate -> save booking.
/// Each step is delegated to a dedicated collaborator (SRP).
/// </summary>
public sealed class BookingParserService(
    IGenerativeAiClient ai,
    IParsedBookingValidator validator,
    IBookingService bookingService,
    IDateTimeProvider clock,
    ILogger<BookingParserService> logger) : IBookingParserService
{
    public const int MaxTextLength = 2000;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<ParseBookingResponse> ParseAndCreateAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ValidationException("Text is required.");
        if (text.Length > MaxTextLength)
            throw new ValidationException($"Text must not exceed {MaxTextLength} characters.");

        var json = await ai.GenerateJsonAsync(
            HotelPrompts.BookingParserSystemInstruction(clock.Today), text.Trim(), BookingJsonSchema.ParsedBooking, ct);

        var parsed = Deserialize(json);
        logger.LogInformation("Gemini parsed booking: {@Parsed}", parsed);

        var command = validator.Validate(parsed);
        var booking = await bookingService.CreateBookingAsync(command, ct);

        return new ParseBookingResponse(parsed, booking);
    }

    private static ParsedBookingDto Deserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ParsedBookingDto>(StripCodeFence(json), JsonOptions)
                   ?? throw new AiInvalidResponseException("Gemini returned an empty JSON document.", json);
        }
        catch (JsonException ex)
        {
            throw new AiInvalidResponseException("Gemini returned invalid JSON.", json, ex);
        }
    }

    /// <summary>Defensive: removes ```json fences in case the model wraps its output.</summary>
    private static string StripCodeFence(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith("```")) return trimmed;
        var firstNewLine = trimmed.IndexOf('\n');
        var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
        return firstNewLine >= 0 && lastFence > firstNewLine
            ? trimmed[(firstNewLine + 1)..lastFence].Trim()
            : trimmed.Trim('`');
    }
}
