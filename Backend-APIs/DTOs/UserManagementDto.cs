using System.ComponentModel.DataAnnotations;

namespace Backend_APIs.DTOs
{
    public class CreateUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        public string Role { get; set; } = null!; // Student, Faculty, Doctor, Admin

        public string? Department { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsEmailVerified { get; set; } = true;

        // Doctor specific fields (optional)
        public string? Specialization { get; set; }
        public int? ExperienceYears { get; set; }
        public string? LicenseNumber { get; set; }
        public string? Bio { get; set; }
    }

    public class UpdateUserDto
    {
        public string FullName { get; set; } = null!;
        public string? Department { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }

         // Doctor specific fields
        public string? Specialization { get; set; }
        public int? ExperienceYears { get; set; }
        public string? LicenseNumber { get; set; }
        public string? Bio { get; set; }
    }
}
