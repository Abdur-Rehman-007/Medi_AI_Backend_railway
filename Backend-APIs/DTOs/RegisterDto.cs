namespace Backend_APIs.DTOs
{
    public class RegisterDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = "Student";
        public string? Department { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }

        // Doctor profile fields (required when Role = Doctor)
        public string? Specialization { get; set; }
        public string? LicenseNumber { get; set; }
        public string? Qualification { get; set; }
        public int? Experience { get; set; }
        public string? RoomNumber { get; set; }
        public string? Bio { get; set; }

    }
}
