namespace BattleGame.Functions.DTOs;

public sealed record ApiError(string Message, IReadOnlyDictionary<string, string[]>? Errors = null);
