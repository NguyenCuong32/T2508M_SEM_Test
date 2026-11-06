using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SunriseHotel.Api.Models;

[Table("Booking")]
public class Booking
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BookingId { get; set; }

    [Required]
    public int RoomId { get; set; }

    [Required]
    [MaxLength(128)]
    public string GuestName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "date")]
    public DateOnly CheckIn { get; set; }

    [Required]
    [Column(TypeName = "date")]
    public DateOnly CheckOut { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    [Required]
    [MaxLength(16)]
    public string Status { get; set; } = "Confirmed"; // Confirmed / Cancelled

    [ForeignKey(nameof(RoomId))]
    public Room? Room { get; set; }
}
