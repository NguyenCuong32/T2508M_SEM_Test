using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BattleGame.Api.Models;

[Table("Player")]
public class Player
{
    [Key]
    public Guid PlayerId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(64)]
    public string PlayerName { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? FullName { get; set; }

    [MaxLength(10)]
    public string? Age { get; set; }

    [Column("Level")]
    public int Level { get; set; } = 1;

    [MaxLength(64)]
    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual ICollection<PlayerAsset> PlayerAssets { get; set; } = new List<PlayerAsset>();
}
