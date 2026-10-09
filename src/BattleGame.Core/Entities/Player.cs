namespace BattleGame.Core.Entities;

public class Player
{
    public Guid PlayerId { get; set; } = Guid.NewGuid();

    public string PlayerName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Age { get; set; } = string.Empty;

    public int Level { get; set; }

    public string Email { get; set; } = string.Empty;

    // Navigation property
    public ICollection<PlayerAsset> PlayerAssets { get; set; } = new List<PlayerAsset>();
}
