namespace BattleGame.DTOs;

public sealed class PlayerAssetReportDto
{
    public int No { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Age { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
}
