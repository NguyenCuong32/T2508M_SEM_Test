using System.ComponentModel.DataAnnotations;

namespace BattleGame.Services;

internal static class InputValidation
{
    public static string RequiredText(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException($"{field} is required.");
        var trimmedValue = value.Trim();
        if (trimmedValue.Length > maxLength)
            throw new ValidationException($"{field} must not exceed {maxLength} characters.");
        return trimmedValue;
    }
}
