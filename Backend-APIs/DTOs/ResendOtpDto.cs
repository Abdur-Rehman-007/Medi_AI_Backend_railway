using System.ComponentModel.DataAnnotations;

namespace Backend_APIs.DTOs
{
    /// <summary>
    /// DTO for resend OTP request
    /// </summary>
    public class ResendOtpDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; } = null!;
    }
}
