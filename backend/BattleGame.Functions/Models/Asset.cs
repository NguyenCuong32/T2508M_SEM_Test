namespace BattleGame.Functions.Models;

public sealed class Asset
{
    public Guid AssetId { get; set; }
    public required string AssetName { get; set; }
    public int LevelRequire { get; set; }

    public ICollection<PlayerAsset> PlayerAssets { get; set; } = new List<PlayerAsset>();
}
