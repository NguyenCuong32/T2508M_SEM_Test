using System.Text.Json;
using System.Text.Json.Nodes;
using SunriseHotel.Application.Abstractions.Ai;

namespace SunriseHotel.Infrastructure.Ai.Gemini;

/// <summary>
/// Keeps the conversation history ("contents") for one Function Calling exchange.
/// The model turn is appended exactly as returned (including thoughtSignature parts), as required by Gemini.
/// </summary>
internal sealed class GeminiChatSession(
    GeminiClient client,
    string systemInstruction,
    IReadOnlyList<AiFunctionDeclaration> functions) : IAiChatSession
{
    private readonly List<JsonNode> _contents = [];

    public Task<AiChatResponse> SendMessageAsync(string message, CancellationToken ct = default)
    {
        _contents.Add(GeminiClient.UserText(message));
        return SendAsync(ct);
    }

    public Task<AiChatResponse> SendFunctionResultsAsync(IReadOnlyList<AiFunctionResult> results,
        CancellationToken ct = default)
    {
        var parts = new JsonArray(results.Select(r =>
        {
            var functionResponse = new JsonObject
            {
                ["name"] = r.Name,
                ["response"] = new JsonObject
                {
                    ["result"] = JsonSerializer.SerializeToNode(r.Result, GeminiClient.JsonOptions)
                }
            };
            if (!string.IsNullOrEmpty(r.Id)) functionResponse["id"] = r.Id;
            return (JsonNode)new JsonObject { ["functionResponse"] = functionResponse };
        }).ToArray());

        _contents.Add(new JsonObject { ["role"] = "user", ["parts"] = parts });
        return SendAsync(ct);
    }

    private async Task<AiChatResponse> SendAsync(CancellationToken ct)
    {
        var request = client.BuildRequest(systemInstruction, _contents, functions);
        var response = await client.SendAsync(request, ct);

        var modelContent = GeminiClient.GetCandidateContent(response);
        _contents.Add(modelContent.DeepClone());

        var calls = (modelContent["parts"] as JsonArray ?? [])
            .OfType<JsonObject>()
            .Select(p => p["functionCall"] as JsonObject)
            .Where(fc => fc is not null)
            .Select(fc => new AiFunctionCall(
                fc!["id"]?.GetValue<string>(),
                fc["name"]?.GetValue<string>() ?? string.Empty,
                JsonSerializer.SerializeToElement(fc["args"] ?? new JsonObject())))
            .ToList();

        return new AiChatResponse(calls.Count == 0 ? GeminiClient.ExtractText(response) : null, calls);
    }
}
