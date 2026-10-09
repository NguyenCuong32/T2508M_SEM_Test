using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BattleGame.Functions.Models
{
    [Table("Asset")]
    public class Asset
    {
        [Key]
        public Guid AssetId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(64)]
        public string AssetName { get; set; } = string.Empty;

        public int LevelRequire { get; set; } = 1;

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<PlayerAsset> PlayerAssets { get; set; } = new List<PlayerAsset>();
    }
}
