namespace BattleGame.Functions.DTOs;

public sealed record RegisterPlayerRequest(
    string? PlayerName,
    string? FullName,
    int Age,
    int Level,
    string? Email);
