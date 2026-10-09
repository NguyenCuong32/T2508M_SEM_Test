namespace BattleGame.Functions.Models;

public sealed class Player
{
    public Guid PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public required string FullName { get; set; }
    public int Age { get; set; }
    public int Level { get; set; } = 1;
    public required string Email { get; set; }

    public ICollection<PlayerAsset> PlayerAssets { get; set; } = new List<PlayerAsset>();
}
