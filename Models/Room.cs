using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace baithi.Models;

[Table("Room")]
public class Room
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int RoomId { get; set; }

    [Required]
    [MaxLength(10)]
    public string RoomNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string RoomType { get; set; } = string.Empty; // Single / Double / Suite

    [Column(TypeName = "decimal(18,2)")]
    public decimal PricePerNight { get; set; }

    public int MaxGuests { get; set; }

    [JsonIgnore]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

