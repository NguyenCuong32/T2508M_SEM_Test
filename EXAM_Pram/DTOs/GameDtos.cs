using System.ComponentModel.DataAnnotations;

namespace EXAM_Pram.DTOs
{
    public class RegisterPlayerDto
    {
        [Required(ErrorMessage = "PlayerName is required")]
        [MaxLength(64)]
        public string PlayerName { get; set; } = string.Empty;

        [MaxLength(128)]
        public string? FullName { get; set; }

        [MaxLength(10)]
        public string? Age { get; set; }

        public int Level { get; set; } = 1;

        [EmailAddress(ErrorMessage = "Invalid Email address format")]
        [MaxLength(64)]
        public string? Email { get; set; }
    }

    public class CreateAssetDto
    {
        [Required(ErrorMessage = "AssetName is required")]
        [MaxLength(64)]
        public string AssetName { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "LevelRequire must be at least 1")]
        public int LevelRequire { get; set; } = 1;
    }

    public class PlayerAssetReportDto
    {
        public int No { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int Level { get; set; }
        public string? Age { get; set; }
        public string AssetName { get; set; } = string.Empty;
    }

    public class AssignAssetDto
    {
        [Required]
        public Guid PlayerId { get; set; }

        [Required]
        public Guid AssetId { get; set; }
    }
}
