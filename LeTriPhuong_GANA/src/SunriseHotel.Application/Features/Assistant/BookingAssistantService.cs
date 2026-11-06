using System.Text.Json;
using Microsoft.Extensions.Logging;
using SunriseHotel.Application.Abstractions.Ai;
using SunriseHotel.Application.Abstractions.Time;
using SunriseHotel.Application.Common.Exceptions;
using SunriseHotel.Application.Prompts;
using SunriseHotel.Domain.Exceptions;

namespace SunriseHotel.Application.Features.Assistant;

public sealed record FunctionCallLog(string Name, JsonElement Arguments, object? Result);

public sealed record AssistantChatResponse(string Message, string Reply, IReadOnlyList<FunctionCallLog> FunctionCalls);

public interface IBookingAssistantService
{
    Task<AssistantChatResponse> ChatAsync(string message, CancellationToken ct = default);
}

/// <summary>
/// Requirement 3: Function Calling loop.
/// message -> Gemini -> (functionCall -> execute C# -> functionResponse -> Gemini)* -> final answer.
/// </summary>
public sealed class BookingAssistantService(
    IGenerativeAiClient ai,
    IEnumerable<IAssistantFunction> functions,
    IDateTimeProvider clock,
    ILogger<BookingAssistantService> logger) : IBookingAssistantService
{
    public const int MaxMessageLength = 1000;
    private const int MaxFunctionRounds = 5;

    private readonly Dictionary<string, IAssistantFunction> _functions =
        functions.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);

    public async Task<AssistantChatResponse> ChatAsync(string message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ValidationException("Message is required.");
        if (message.Length > MaxMessageLength)
            throw new ValidationException($"Message must not exceed {MaxMessageLength} characters.");

        var chat = ai.StartChat(
            HotelPrompts.BookingAssistantSystemInstruction(clock.Today),
            _functions.Values.Select(f => f.Declaration).ToList());

        var log = new List<FunctionCallLog>();
        var response = await chat.SendMessageAsync(message.Trim(), ct);

        for (var round = 0; response.HasFunctionCalls; round++)
        {
            if (round >= MaxFunctionRounds)
                throw new AiServiceException($"The assistant exceeded {MaxFunctionRounds} function-calling rounds.");

            var results = new List<AiFunctionResult>();
            foreach (var call in response.FunctionCalls)
            {
                var result = await ExecuteAsync(call, ct);
                log.Add(new FunctionCallLog(call.Name, call.Arguments, result));
                results.Add(new AiFunctionResult(call.Id, call.Name, result));
            }

            response = await chat.SendFunctionResultsAsync(results, ct);
        }

        return new AssistantChatResponse(message.Trim(), response.Text?.Trim() ?? string.Empty, log);
    }

    /// <summary>Executes one call. Business errors are returned to Gemini as { error } so it can explain them.</summary>
    private async Task<object> ExecuteAsync(AiFunctionCall call, CancellationToken ct)
    {
        logger.LogInformation("Gemini requested {Function}({Args})", call.Name, call.Arguments.GetRawText());

        if (!_functions.TryGetValue(call.Name, out var function))
            return new { error = $"Unknown function '{call.Name}'." };

        try
        {
            return await function.ExecuteAsync(call.Arguments, ct);
        }
        catch (ValidationException ex)
        {
            return new { error = string.Join(" ", ex.Errors) };
        }
        catch (Exception ex) when (ex is NotFoundException or ConflictException or DomainException)
        {
            return new { error = ex.Message };
        }
    }
}
