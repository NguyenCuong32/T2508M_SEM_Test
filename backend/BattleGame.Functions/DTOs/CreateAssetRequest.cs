using System.ComponentModel.DataAnnotations;

namespace BattleGame.Functions.DTOs
{
    public class CreateAssetRequest
    {
        [Required(ErrorMessage = "AssetName is required")]
        [StringLength(64, ErrorMessage = "AssetName cannot exceed 64 characters")]
        public string AssetName { get; set; } = string.Empty;

        [Range(1, 1000, ErrorMessage = "LevelRequire must be at least 1")]
        public int LevelRequire { get; set; } = 1;
    }
}
