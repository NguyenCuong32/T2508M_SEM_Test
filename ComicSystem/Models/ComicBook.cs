using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComicSystem.Models
{
    public class ComicBook
    {
        [Key]
        public int ComicBookID { get; set; }
        [Required, StringLength(255)]
        public string Title { get; set; } = string.Empty;
        [StringLength(255)]
        public string? Author { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerDay { get; set; }

        public ICollection<RentalDetail>? RentalDetails { get; set; }
    }
}
