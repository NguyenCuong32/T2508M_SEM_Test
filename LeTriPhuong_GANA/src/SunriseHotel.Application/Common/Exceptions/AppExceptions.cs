namespace SunriseHotel.Application.Common.Exceptions;

/// <summary>Input data is invalid (HTTP 400).</summary>
public class ValidationException(IReadOnlyList<string> errors, object? details = null)
    : Exception("One or more validation errors occurred.")
{
    public ValidationException(string error) : this([error]) { }

    public IReadOnlyList<string> Errors { get; } = errors;

    /// <summary>Optional extra data returned to the client (e.g. the JSON parsed by Gemini).</summary>
    public object? Details { get; } = details;
}

/// <summary>Requested resource does not exist (HTTP 404).</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>Request conflicts with current state, e.g. no room available (HTTP 409).</summary>
public class ConflictException(string message) : Exception(message);
