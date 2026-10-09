using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EXAM_Pram.Models
{
    [Table("PlayerAsset")]
    public class PlayerAsset
    {
        public Guid PlayerId { get; set; }
        public Player? Player { get; set; }

        public Guid AssetId { get; set; }
        public Asset? Asset { get; set; }
    }
}
