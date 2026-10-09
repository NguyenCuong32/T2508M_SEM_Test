namespace BattleGame.Functions.Models;

public sealed class Asset
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<PlayerAsset> PlayerAssets { get; set; } = new List<PlayerAsset>();
}
