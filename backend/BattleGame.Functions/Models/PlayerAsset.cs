using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BattleGame.Functions.Models
{
    [Table("PlayerAsset")]
    public class PlayerAsset
    {
        public Guid PlayerId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [ForeignKey(nameof(PlayerId))]
        public virtual Player? Player { get; set; }

        public Guid AssetId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [ForeignKey(nameof(AssetId))]
        public virtual Asset? Asset { get; set; }
    }
}
