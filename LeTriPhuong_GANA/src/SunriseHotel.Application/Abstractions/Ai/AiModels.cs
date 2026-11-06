using System.Text.Json;
using System.Text.Json.Nodes;

namespace SunriseHotel.Application.Abstractions.Ai;

/// <summary>Provider-neutral description of a function the model may call.</summary>
public sealed record AiFunctionDeclaration(string Name, string Description, JsonNode Parameters);

/// <summary>A function call requested by the model.</summary>
public sealed record AiFunctionCall(string? Id, string Name, JsonElement Arguments);

/// <summary>Result of executing a function call, sent back to the model.</summary>
public sealed record AiFunctionResult(string? Id, string Name, object? Result);

/// <summary>One model turn: either final text or a list of function calls.</summary>
public sealed record AiChatResponse(string? Text, IReadOnlyList<AiFunctionCall> FunctionCalls)
{
    public bool HasFunctionCalls => FunctionCalls.Count > 0;
}
