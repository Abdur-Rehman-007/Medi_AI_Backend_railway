namespace Backend_APIs.DTOs
{
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
        public string Severity { get; set; } = "Low";
        public List<string> Recommendations { get; set; } = new();
        public List<string> WarningSigns { get; set; } = new();
        public string? Answer { get; set; }
    }
}
