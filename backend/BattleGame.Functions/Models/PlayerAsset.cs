namespace BattleGame.Functions.Models;

public sealed class PlayerAsset
{
    public Guid PlayerId { get; set; }
    public Guid AssetId { get; set; }

    public Player Player { get; set; } = null!;
    public Asset Asset { get; set; } = null!;
}
