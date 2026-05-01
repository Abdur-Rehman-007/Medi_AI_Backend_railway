using System.ComponentModel.DataAnnotations;

namespace Backend_APIs.DTOs
{
    public class MedicalHistoryDto
    {
        public int Id { get; set; }
        public string RecordType { get; set; } = null!; // e.g., "Allergy", "Surgery", "Chornic Condition"
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? DiagnosisDate { get; set; } // Send as string YYYY-MM-DD
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateMedicalHistoryDto
    {
        [Required]
        public string RecordType { get; set; } = null!; // e.g., "Allergy", "Surgery", "Chornic Condition"

        [Required]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime? DiagnosisDate { get; set; }

        public string? Notes { get; set; }
    }

    public class UpdateMedicalHistoryDto
    {
        public string? RecordType { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DiagnosisDate { get; set; }
        public string? Notes { get; set; }
    }
}
