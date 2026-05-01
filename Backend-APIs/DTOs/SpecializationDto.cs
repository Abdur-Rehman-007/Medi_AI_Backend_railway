namespace Backend_APIs.DTOs
{
    /// <summary>
    /// DTO for specialization with count
    /// </summary>
    public class SpecializationDto
    {
        public string Specialization { get; set; } = null!;
        public int DoctorCount { get; set; }
    }
}
