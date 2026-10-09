using System.ComponentModel.DataAnnotations;

namespace BattleGame.Api.DTOs;

public class CreateAssetRequest
{
    public Guid? AssetId { get; set; }

    [Required(ErrorMessage = "AssetName is required")]
    [StringLength(64, ErrorMessage = "AssetName must not exceed 64 characters")]
    public string AssetName { get; set; } = string.Empty;

    public int? LevelRequire { get; set; } = 1;
}
