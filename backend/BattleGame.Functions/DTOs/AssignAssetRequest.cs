
using System;
using System.ComponentModel.DataAnnotations;

namespace BattleGame.Functions.DTOs
{
    public class AssignAssetRequest
    {
        [Required]
        public Guid PlayerId { get; set; }

        [Required]
        public Guid AssetId { get; set; }
    }
}
