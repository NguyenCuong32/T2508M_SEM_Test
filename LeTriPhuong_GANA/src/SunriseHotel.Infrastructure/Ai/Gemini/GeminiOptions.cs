namespace SunriseHotel.Infrastructure.Ai.Gemini;

/// <summary>
/// Bound from configuration section "Gemini".
/// ApiKey is NEVER stored in appsettings.json: it comes from User Secrets (dev) or the
/// environment variable Gemini__ApiKey (server / CI).
/// </summary>
public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-3.8-flash";
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/";
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>Retries for transient errors (500/503 "model overloaded"). 429 is never retried.</summary>
    public int MaxRetries { get; set; } = 2;
    public float Temperature { get; set; } = 0.3f;
}
