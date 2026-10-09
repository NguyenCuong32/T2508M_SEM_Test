using System.ComponentModel.DataAnnotations;

namespace BattleGame.Functions.DTOs
{
    public class RegisterPlayerRequest
    {
        [Required(ErrorMessage = "PlayerName is required")]
        [StringLength(64, ErrorMessage = "PlayerName cannot exceed 64 characters")]
        public string PlayerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "FullName is required")]
        [StringLength(128, ErrorMessage = "FullName cannot exceed 128 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Age is required")]
        [StringLength(10, ErrorMessage = "Age cannot exceed 10 characters")]
        public string Age { get; set; } = string.Empty;

        [Range(1, 1000, ErrorMessage = "Level must be greater than or equal to 1")]
        public int Level { get; set; } = 1;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email format")]
        [StringLength(64, ErrorMessage = "Email cannot exceed 64 characters")]
        public string Email { get; set; } = string.Empty;
    }
}
