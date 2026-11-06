using System.Text.Json;
using SunriseHotel.Application.Abstractions.Ai;

namespace SunriseHotel.Application.Features.Assistant;

/// <summary>
/// A C# function exposed to Gemini through Function Calling.
/// Adding a new function = adding a new class + DI registration, no change to the assistant (OCP).
/// </summary>
public interface IAssistantFunction
{
    string Name { get; }
    AiFunctionDeclaration Declaration { get; }
    Task<object> ExecuteAsync(JsonElement arguments, CancellationToken ct = default);
}
