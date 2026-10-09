using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BattleGame.Functions.Models
{
    [Table("Player")]
    public class Player
    {
        [Key]
        public Guid PlayerId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(64)]
        public string PlayerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(128)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Age { get; set; } = string.Empty;

        [Column("Level")]
        public int Level { get; set; } = 1;

        [Required]
        [MaxLength(64)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<PlayerAsset> PlayerAssets { get; set; } = new List<PlayerAsset>();
    }
}
