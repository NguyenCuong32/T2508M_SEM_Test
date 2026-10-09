namespace BattleGame.Functions.DTOs;

public sealed record PlayerAssetReportItem(
    int No,
    Guid PlayerId,
    string PlayerName,
    int Level,
    int Age,
    Guid AssetId,
    string AssetName);
