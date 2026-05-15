namespace Backend_APIs.DTOs
{
    public class AnalyzeSymptomsDto
    {
        public List<string> SelectedSymptoms { get; set; } = new List<string>();
        public string? AdditionalDescription { get; set; }
        public string? Question { get; set; }
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
        public string? Answer { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AiAnalyzeRequestDto
    {
        public List<string> SelectedSymptoms { get; set; } = new();
        public string? AdditionalDescription { get; set; }
        public string? Question { get; set; }
    }

    public class AiAnalyzeResultDto
    {
        public string Condition { get; set; } = "General Health Guidance";
        public double Confidence { get; set; } = 65;
        public string Severity { get; set; } = "Mild";
        public List<string> Recommendations { get; set; } = new();
        public List<string> WarningSigns { get; set; } = new();
        public string? Answer { get; set; }
    }
}
