namespace Backend_APIs.DTOs
{
    /// <summary>
    /// DTO for updating user profile
    /// </summary>
    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? Department { get; set; }
        public string? RegistrationNumber { get; set; }
    }
}
