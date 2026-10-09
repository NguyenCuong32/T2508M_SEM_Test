using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EXAM_Pram.Models
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

        // Navigation property
        public ICollection<PlayerAsset> PlayerAssets { get; set; } = new List<PlayerAsset>();
    }
}
