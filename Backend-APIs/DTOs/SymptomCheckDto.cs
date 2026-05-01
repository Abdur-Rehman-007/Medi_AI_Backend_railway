namespace Backend_APIs.DTOs
{
    public class AnalyzeSymptomsDto
    {
        public List<string> SelectedSymptoms { get; set; } = new List<string>();
        public string? AdditionalDescription { get; set; }
    }

    public class SymptomCheckResponseDto
    {
        public int Id { get; set; }
        public string Symptoms { get; set; } = null!;
        public string Condition { get; set; } = null!;
        public decimal Confidence { get; set; }
        public string Severity { get; set; } = null!;
        public List<string> Recommendations { get; set; } = new List<string>();
        public List<string> WarningSigns { get; set; } = new List<string>(); // Mapped from RecommendedAction or separate
        public DateTime CreatedAt { get; set; }
    }
}
