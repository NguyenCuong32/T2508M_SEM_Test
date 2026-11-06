namespace SunriseHotel.Application.Common.Exceptions;

/// <summary>Generic failure while calling the AI provider (HTTP 502/503).</summary>
public class AiServiceException(string message, Exception? inner = null) : Exception(message, inner);

/// <summary>AI provider rejected the call because the quota / rate limit was exceeded (HTTP 429).</summary>
public class AiQuotaExceededException(string message, TimeSpan? retryAfter = null) : AiServiceException(message)
{
    public TimeSpan? RetryAfter { get; } = retryAfter;
}

/// <summary>AI provider answered but the content is unusable, e.g. invalid JSON (HTTP 502).</summary>
public class AiInvalidResponseException(string message, string? rawResponse = null, Exception? inner = null)
    : AiServiceException(message, inner)
{
    public string? RawResponse { get; } = rawResponse;
}

/// <summary>AI provider is not configured, e.g. missing API key (HTTP 500).</summary>
public class AiConfigurationException(string message) : AiServiceException(message);
