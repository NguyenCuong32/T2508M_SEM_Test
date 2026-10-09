namespace BattleGame.Core.Dtos;

public class RegisterPlayerDto
{
    public string PlayerName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Age { get; set; } = string.Empty;

    public int Level { get; set; }

    public string Email { get; set; } = string.Empty;
}
