using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BattleGame.Api.Models;

[Table("Asset")]
public class Asset
{
    [Key]
    public Guid AssetId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(64)]
    public string AssetName { get; set; } = string.Empty;

    public int LevelRequire { get; set; } = 1;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual ICollection<PlayerAsset> PlayerAssets { get; set; } = new List<PlayerAsset>();
}
