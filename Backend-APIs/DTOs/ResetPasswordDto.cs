using System.ComponentModel.DataAnnotations;

namespace Backend_APIs.DTOs
{
    /// <summary>
    /// DTO for reset password request
    /// </summary>
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Reset token is required")]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string NewPassword { get; set; } = null!;
    }
}
