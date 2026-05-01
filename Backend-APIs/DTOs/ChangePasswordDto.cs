namespace Backend_APIs.DTOs
{
    /// <summary>
    /// DTO for changing password
    /// </summary>
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
