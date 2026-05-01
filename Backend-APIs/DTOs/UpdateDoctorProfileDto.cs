using System.ComponentModel.DataAnnotations;

namespace Backend_APIs.DTOs
{
    public class UpdateDoctorProfileDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Specialization { get; set; }
        public string? RoomNumber { get; set; }
        public string? Bio { get; set; }
        public bool? IsAvailable { get; set; }
    }
}