using System.ComponentModel.DataAnnotations;

namespace Backend_APIs.DTOs
{
    /// <summary>
    /// All three fields must match the same account in the database.
    /// No email/OTP is sent — the reset token is returned directly.
    /// </summary>
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "CMS / Registration number is required")]
        public string RegistrationNumber { get; set; } = null!;
    }
}
