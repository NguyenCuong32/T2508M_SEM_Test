namespace BattleGameFunction.Models;

public class Asset
{
    public string AssetId { get; set; } = Guid.NewGuid().ToString();
    public string AssetName { get; set; } = string.Empty;
    public int LevelRequire { get; set; }

    public ICollection<PlayerAsset> PlayerAssets { get; set; } = new List<PlayerAsset>();
}
