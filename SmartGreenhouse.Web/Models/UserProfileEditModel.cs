using System.ComponentModel.DataAnnotations;

public class UserProfileEditModel
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FullName { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*\W).{8,16}$",
        ErrorMessage = "Password must be 8-16 chars, include 1 digit, 1 symbol, 1 capital letter.")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Phone must be in Ukrainian format +380XXXXXXXXX.")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

