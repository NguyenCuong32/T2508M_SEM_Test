namespace BattleGame.Core.Entities;

public class Asset
{
    public Guid AssetId { get; set; } = Guid.NewGuid();

    public string AssetName { get; set; } = string.Empty;

    public int LevelRequire { get; set; }

    // Navigation property
    public ICollection<PlayerAsset> PlayerAssets { get; set; } = new List<PlayerAsset>();
}
