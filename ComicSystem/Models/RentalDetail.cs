using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComicSystem.Models
{
    public class RentalDetail
    {
        [Key]
        public int RentalDetailID { get; set; }
        public int RentalID { get; set; }
        public int ComicBookID { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerDay { get; set; }

        public Rental? Rental { get; set; }
        public ComicBook? ComicBook { get; set; }
    }
}
