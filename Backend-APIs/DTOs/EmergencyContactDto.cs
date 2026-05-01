using System.ComponentModel.DataAnnotations;

namespace Backend_APIs.DTOs
{
    public class EmergencyContactDto
    {
        public int Id { get; set; }
        public string ContactName { get; set; } = null!;
        public string Relationship { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; } // May not exist on model, check later
    }

    public class CreateEmergencyContactDto
    {
        [Required]
        public string ContactName { get; set; } = null!;

        [Required]
        public string Relationship { get; set; } = null!;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = null!;

        [EmailAddress]
        public string? Email { get; set; }

        public string? Address { get; set; }
    }


    public class UpdateEmergencyContactDto
    {
        public string? Name { get; set; }
        public string? Relation { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
