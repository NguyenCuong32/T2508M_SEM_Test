namespace BattleGame.DTOs;

public sealed record ApiResponseDto<T>(bool Success, string Message, T? Data);
