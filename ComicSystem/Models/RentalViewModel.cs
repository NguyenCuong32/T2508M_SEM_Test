using System.ComponentModel.DataAnnotations;

namespace ComicSystem.Models
{
    public class RentalViewModel
    {
        [Required]
        public int CustomerID { get; set; }

        [Required]
        public int ComicBookID { get; set; }

        [Required]
        public DateTime RentalDate { get; set; } = DateTime.Today;

        [Required]
        public DateTime ReturnDate { get; set; } = DateTime.Today.AddDays(7);

        [Required]
        [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100")]
        public int Quantity { get; set; } = 1;
    }
}
