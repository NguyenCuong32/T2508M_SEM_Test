namespace BattleGame.Api.Dtos;

public record RegisterPlayerRequest(string? PlayerName, string? FullName, string? Age, int? Level, string? Email);

public record CreateAssetRequest(string? AssetName, int? LevelRequire);

public record PlayerAssetReportRow(int No, string PlayerName, int Level, string Age, string AssetName);

public record ErrorResponse(string Message, IReadOnlyList<string>? Errors = null);
