using System.ComponentModel.DataAnnotations;

namespace SmartGreenhouse.Web.Models
{
    public class UserRegistrationModel
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [StringLength(500)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(16, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one digit and one special character.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Phone must be in +380XXXXXXXXX format.")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
