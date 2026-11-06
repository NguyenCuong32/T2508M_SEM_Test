using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SunriseHotel.Application.Abstractions.Ai;
using SunriseHotel.Application.Common.Exceptions;

namespace SunriseHotel.Infrastructure.Ai.Gemini;

/// <summary>
/// Calls the Gemini REST API (generateContent) directly with HttpClient.
/// The API key is sent in the "x-goog-api-key" header (never in the URL, so it is not written to logs).
/// </summary>
public sealed partial class GeminiClient(
    HttpClient http,
    IOptions<GeminiOptions> options,
    ILogger<GeminiClient> logger) : IGenerativeAiClient
{
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly GeminiOptions _options = options.Value;

    public async Task<string> GenerateTextAsync(string systemInstruction, string prompt, CancellationToken ct = default)
    {
        var request = BuildRequest(systemInstruction, [UserText(prompt)]);
        var response = await SendAsync(request, ct);
        var text = ExtractText(response);
        return string.IsNullOrWhiteSpace(text)
            ? throw new AiInvalidResponseException($"Gemini returned an empty answer (finishReason: {FinishReason(response)}).")
            : text;
    }

    public async Task<string> GenerateJsonAsync(string systemInstruction, string prompt, string responseSchemaJson,
        CancellationToken ct = default)
    {
        var request = BuildRequest(systemInstruction, [UserText(prompt)]);
        var config = (JsonObject)request["generationConfig"]!;
        config["temperature"] = 0;
        config["responseMimeType"] = "application/json"; // JSON mode
        config["responseSchema"] = JsonNode.Parse(responseSchemaJson); // structured output

        var response = await SendAsync(request, ct);
        var text = ExtractText(response);
        return string.IsNullOrWhiteSpace(text)
            ? throw new AiInvalidResponseException($"Gemini returned no JSON (finishReason: {FinishReason(response)}).")
            : text;
    }

    public IAiChatSession StartChat(string systemInstruction, IReadOnlyList<AiFunctionDeclaration> functions) =>
        new GeminiChatSession(this, systemInstruction, functions);

    // ---------------------------------------------------------------- request building

    internal JsonObject BuildRequest(string systemInstruction, IEnumerable<JsonNode> contents,
        IReadOnlyList<AiFunctionDeclaration>? functions = null)
    {
        var request = new JsonObject
        {
            ["systemInstruction"] = new JsonObject { ["parts"] = new JsonArray(new JsonObject { ["text"] = systemInstruction }) },
            ["contents"] = new JsonArray(contents.Select(c => c.DeepClone()).ToArray()),
            ["generationConfig"] = new JsonObject { ["temperature"] = _options.Temperature }
        };

        if (functions is { Count: > 0 })
        {
            var declarations = new JsonArray(functions.Select(f => (JsonNode)new JsonObject
            {
                ["name"] = f.Name,
                ["description"] = f.Description,
                ["parameters"] = f.Parameters.DeepClone()
            }).ToArray());

            request["tools"] = new JsonArray(new JsonObject { ["functionDeclarations"] = declarations });
            request["toolConfig"] = new JsonObject
            {
                ["functionCallingConfig"] = new JsonObject { ["mode"] = "AUTO" }
            };
        }

        return request;
    }

    internal static JsonObject UserText(string text) => new()
    {
        ["role"] = "user",
        ["parts"] = new JsonArray(new JsonObject { ["text"] = text })
    };

    // ---------------------------------------------------------------- HTTP + error handling

    internal async Task<JsonObject> SendAsync(JsonObject request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new AiConfigurationException(
                "Gemini API key is not configured. Set it with: dotnet user-secrets set \"Gemini:ApiKey\" \"<key>\" " +
                "or the environment variable Gemini__ApiKey.");

        var body = request.ToJsonString();

        for (var attempt = 0; ; attempt++)
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, $"models/{_options.Model}:generateContent");
            message.Headers.Add("x-goog-api-key", _options.ApiKey);
            message.Content = new StringContent(body, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await http.SendAsync(message, ct);
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                throw new AiServiceException("Gemini did not respond in time (timeout).", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new AiServiceException("Cannot reach the Gemini API. Check the network connection.", ex);
            }

            using (response)
            {
                var content = await response.Content.ReadAsStringAsync(ct);

                if (response.IsSuccessStatusCode)
                    return ParseBody(content);

                var isTransient = response.StatusCode is HttpStatusCode.InternalServerError
                    or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout;

                if (isTransient && attempt < _options.MaxRetries)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    logger.LogWarning("Gemini returned {Status}, retrying in {Delay}s", (int)response.StatusCode, delay.TotalSeconds);
                    await Task.Delay(delay, ct);
                    continue;
                }

                throw MapError(response.StatusCode, response.Headers.RetryAfter, content);
            }
        }
    }

    private static JsonObject ParseBody(string content)
    {
        try
        {
            return JsonNode.Parse(content) as JsonObject
                   ?? throw new AiInvalidResponseException("Gemini response is not a JSON object.", content);
        }
        catch (JsonException ex)
        {
            throw new AiInvalidResponseException("Gemini response is not valid JSON.", content, ex);
        }
    }

    private Exception MapError(HttpStatusCode status, RetryConditionHeaderValue? retryAfterHeader, string content)
    {
        var apiMessage = TryGetErrorMessage(content);
        logger.LogWarning("Gemini API error {Status}: {Message}", (int)status, apiMessage);

        return status switch
        {
            HttpStatusCode.TooManyRequests => new AiQuotaExceededException(
                "Gemini quota / rate limit exceeded (429). Please wait and try again.",
                retryAfterHeader?.Delta ?? TryGetRetryDelay(content)),
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new AiConfigurationException(
                "Gemini rejected the API key (401/403). Check the configured key."),
            HttpStatusCode.BadRequest when apiMessage.Contains("API key", StringComparison.OrdinalIgnoreCase) =>
                new AiConfigurationException("Gemini API key is invalid."),
            HttpStatusCode.NotFound => new AiConfigurationException(
                $"Gemini model '{_options.Model}' is not available: {apiMessage}"),
            _ => new AiServiceException($"Gemini API error {(int)status}: {apiMessage}")
        };
    }

    private static string TryGetErrorMessage(string content)
    {
        try
        {
            return JsonNode.Parse(content)?["error"]?["message"]?.GetValue<string>() ?? content;
        }
        catch (JsonException)
        {
            return content.Length > 300 ? content[..300] : content;
        }
    }

    [GeneratedRegex("\"retryDelay\"\\s*:\\s*\"(\\d+)(?:\\.\\d+)?s\"")]
    private static partial Regex RetryDelayRegex();

    private static TimeSpan? TryGetRetryDelay(string content)
    {
        var match = RetryDelayRegex().Match(content);
        return match.Success ? TimeSpan.FromSeconds(int.Parse(match.Groups[1].Value)) : null;
    }

    // ---------------------------------------------------------------- response parsing

    internal static JsonObject GetCandidateContent(JsonObject response)
    {
        var blockReason = response["promptFeedback"]?["blockReason"]?.GetValue<string>();
        if (blockReason is not null)
            throw new AiInvalidResponseException($"Gemini blocked the request (blockReason: {blockReason}).");

        return response["candidates"]?[0]?["content"] as JsonObject
               ?? throw new AiInvalidResponseException(
                   $"Gemini returned no content (finishReason: {FinishReason(response)}).", response.ToJsonString());
    }

    internal static string ExtractText(JsonObject response)
    {
        var parts = GetCandidateContent(response)["parts"] as JsonArray ?? [];
        return string.Concat(parts
            .OfType<JsonObject>()
            .Where(p => p["thought"]?.GetValue<bool>() != true)
            .Select(p => p["text"]?.GetValue<string>() ?? string.Empty));
    }

    internal static string FinishReason(JsonObject response) =>
        response["candidates"]?[0]?["finishReason"]?.GetValue<string>() ?? "unknown";
}
