namespace SunriseHotel.Application.Abstractions.Ai;

/// <summary>
/// Abstraction over the LLM provider (Gemini). The application layer depends on this
/// interface only, so the provider can be swapped without touching business logic (DIP).
/// </summary>
public interface IGenerativeAiClient
{
    Task<string> GenerateTextAsync(string systemInstruction, string prompt, CancellationToken ct = default);

    /// <summary>Asks the model for JSON only, constrained by an OpenAPI-style response schema.</summary>
    Task<string> GenerateJsonAsync(string systemInstruction, string prompt, string responseSchemaJson,
        CancellationToken ct = default);

    /// <summary>Starts a multi-turn conversation in which the model may call the given functions.</summary>
    IAiChatSession StartChat(string systemInstruction, IReadOnlyList<AiFunctionDeclaration> functions);
}

public interface IAiChatSession
{
    Task<AiChatResponse> SendMessageAsync(string message, CancellationToken ct = default);
    Task<AiChatResponse> SendFunctionResultsAsync(IReadOnlyList<AiFunctionResult> results, CancellationToken ct = default);
}
