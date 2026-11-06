using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SunriseHotel.Models
{
    [Table("Rooms")]
    public class Room
    {
        [Key]
        public int roomId { get; set; }

        [Required]
        [StringLength(10)]
        public string roomNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(32)]
        public string roomType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerNight { get; set; }
        public int MaxGuests { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
