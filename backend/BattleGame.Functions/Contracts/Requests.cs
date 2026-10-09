namespace BattleGame.Functions.Contracts;

public sealed record RegisterPlayerRequest(string PlayerName, string FullName, int Age, int CurrentLevel = 1);
public sealed record CreateAssetRequest(string Name, string Type, string? Description = null);
public sealed record AssignAssetRequest(int PlayerId, int AssetId);
