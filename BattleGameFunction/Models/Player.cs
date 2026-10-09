namespace BattleGameFunction.Models;

public class Player
{
    public string PlayerId { get; set; } = Guid.NewGuid().ToString();
    public string PlayerName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Age { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Email { get; set; } = string.Empty;

    public ICollection<PlayerAsset> PlayerAssets { get; set; } = new List<PlayerAsset>();
}
