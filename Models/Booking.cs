using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SunriseHotel.Models
{
    [Table("Bookings")]
    public class Booking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(128)]
        public string GuestName { get; set; } = string.Empty;

        [Column(TypeName = "date")]
        public DateTime CheckIn { get; set; }
        [Column(TypeName = "date")]
        public DateTime CheckOut { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        public Room Room { get; set; } = null!;
    }
}
