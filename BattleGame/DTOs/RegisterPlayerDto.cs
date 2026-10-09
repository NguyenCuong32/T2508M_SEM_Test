namespace BattleGame.DTOs;

public sealed record RegisterPlayerDto(string PlayerName, string FullName, string Age, int Level, string Email);
