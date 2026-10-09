using System.Net.Mail;
using BattleGame.Api.Dtos;

namespace BattleGame.Api.Services;

public static class Validation
{
    public static List<string> Validate(RegisterPlayerRequest? r)
    {
        var errors = new List<string>();
        if (r is null) return ["Request body is required."];
        Required(errors, r.PlayerName, "playerName", 64);
        Required(errors, r.FullName, "fullName", 128);
        Required(errors, r.Age, "age", 10);
        Required(errors, r.Email, "email", 64);
        if (r.Level is null || r.Level < 0) errors.Add("level must be an integer >= 0.");
        if (!string.IsNullOrWhiteSpace(r.Email) && !MailAddress.TryCreate(r.Email, out _))
            errors.Add("email is not a valid address.");
        return errors;
    }

    public static List<string> Validate(CreateAssetRequest? r)
    {
        var errors = new List<string>();
        if (r is null) return ["Request body is required."];
        Required(errors, r.AssetName, "assetName", 64);
        if (r.LevelRequire is null || r.LevelRequire < 0) errors.Add("levelRequire must be an integer >= 0.");
        return errors;
    }

    private static void Required(List<string> errors, string? value, string name, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) errors.Add($"{name} is required.");
        else if (value.Trim().Length > max) errors.Add($"{name} must be at most {max} characters.");
    }
}
