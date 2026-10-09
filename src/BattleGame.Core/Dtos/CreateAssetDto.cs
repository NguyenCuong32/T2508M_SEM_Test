namespace BattleGame.Core.Dtos;

public class CreateAssetDto
{
    public string AssetName { get; set; } = string.Empty;

    public int LevelRequire { get; set; }
}
