namespace BattleGame.Core.Dtos;

public class PlayerAssetReportDto
{
    public int No { get; set; }

    public Guid PlayerId { get; set; }

    public string PlayerName { get; set; } = string.Empty;

    public int Level { get; set; }

    public string Age { get; set; } = string.Empty;

    public Guid? AssetId { get; set; }

    public string AssetName { get; set; } = string.Empty;
}
