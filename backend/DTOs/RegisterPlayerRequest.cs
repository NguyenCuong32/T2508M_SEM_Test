using System.ComponentModel.DataAnnotations;

namespace BattleGame.Api.DTOs;

public class RegisterPlayerRequest
{
    public Guid? PlayerId { get; set; }

    [Required(ErrorMessage = "PlayerName is required")]
    [StringLength(64, ErrorMessage = "PlayerName must not exceed 64 characters")]
    public string PlayerName { get; set; } = string.Empty;

    [StringLength(128, ErrorMessage = "FullName must not exceed 128 characters")]
    public string? FullName { get; set; }

    [StringLength(10, ErrorMessage = "Age must not exceed 10 characters")]
    public string? Age { get; set; }

    public int? Level { get; set; } = 1;

    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(64, ErrorMessage = "Email must not exceed 64 characters")]
    public string? Email { get; set; }
}
