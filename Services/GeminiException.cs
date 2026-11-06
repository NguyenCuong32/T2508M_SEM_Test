namespace SunriseHotel.Services;

public class GeminiException(string message, int status = 502) : Exception(message) { public int Status { get; } = status; }
