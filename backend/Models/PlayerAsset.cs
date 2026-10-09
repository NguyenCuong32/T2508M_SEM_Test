using System.ComponentModel.DataAnnotations.Schema;

namespace BattleGame.Api.Models;

[Table("PlayerAsset")]
public class PlayerAsset
{
    public Guid PlayerId { get; set; }
    public virtual Player Player { get; set; } = null!;

    public Guid AssetId { get; set; }
    public virtual Asset Asset { get; set; } = null!;

    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
}
