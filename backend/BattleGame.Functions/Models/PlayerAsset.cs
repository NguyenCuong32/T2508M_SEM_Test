namespace BattleGame.Functions.Models;

public sealed class PlayerAsset
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;
    public DateTime AcquiredAt { get; set; }
}
