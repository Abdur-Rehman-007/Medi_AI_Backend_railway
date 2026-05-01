namespace Backend_APIs.DTOs
{
    /// <summary>
    /// DTO for doctor search results
    /// </summary>
    public class DoctorSearchDto
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public string Specialization { get; set; } = null!;
        public string? Qualifications { get; set; }
        public int? ExperienceYears { get; set; }
        public bool IsAvailable { get; set; }
        public double? AverageRating { get; set; }
        public int? TotalReviews { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Bio { get; set; }
    }
}
