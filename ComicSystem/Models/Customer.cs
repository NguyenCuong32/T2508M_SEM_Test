using System.ComponentModel.DataAnnotations;

namespace ComicSystem.Models
{
    public class Customer
    {
        [Key]
        public int CustomerID { get; set; }
        [Required, StringLength(255)]
        public string FullName { get; set; } = string.Empty;
        [Required, StringLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public ICollection<Rental>? Rentals { get; set; }
    }
}
