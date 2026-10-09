namespace BattleGameFunction.Models;

public class PlayerAsset
{
    public string PlayerId { get; set; } = string.Empty;
    public Player? Player { get; set; }

    public string AssetId { get; set; } = string.Empty;
    public Asset? Asset { get; set; }
}
