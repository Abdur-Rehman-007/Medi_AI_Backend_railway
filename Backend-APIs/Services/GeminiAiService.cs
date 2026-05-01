using Backend_APIs.DTOs;
using System.Text;
using System.Text.Json;

namespace Backend_APIs.Services
{
    public class GeminiAiService : IGeminiAiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public GeminiAiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<AiAnalyzeResultDto> AnalyzeAsync(AiAnalyzeRequestDto request, CancellationToken cancellationToken = default)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var model = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return BuildFallback(request, "Gemini API key is not configured.");
            }

            try
            {
                var combinedInput = BuildInput(request);
                var prompt = $@"You are a healthcare triage assistant for BUITEMS university users.
Return ONLY valid JSON with this exact schema:
{{
    ""condition"": ""string"",
    ""confidence"": 0,
    ""severity"": ""Low|Moderate|High"",
    ""recommendations"": [""string""],
    ""warningSigns"": [""string""],
    ""answer"": ""short plain language response""
}}
Symptoms/question: {combinedInput}
Keep advice general and safe. Never provide a final diagnosis.";

                var payload = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[] { new { text = prompt } }
                        }
                    }
                };

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
                var client = _httpClientFactory.CreateClient();

                using var response = await client.PostAsync(
                    url,
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var failedBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    return BuildFallback(request, $"Gemini API request failed: {(int)response.StatusCode}. {failedBody}");
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var text = ExtractTextFromGemini(responseBody);
                if (string.IsNullOrWhiteSpace(text))
                {
                    return BuildFallback(request, "Gemini returned an empty response.");
                }

                var json = TryExtractJson(text);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return BuildFallback(request, text);
                }

                var parsed = JsonSerializer.Deserialize<AiAnalyzeResultDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (parsed == null)
                {
                    return BuildFallback(request, text);
                }

                parsed.Recommendations ??= new List<string>();
                parsed.WarningSigns ??= new List<string>();
                parsed.Condition = string.IsNullOrWhiteSpace(parsed.Condition) ? "General Health Guidance" : parsed.Condition;
                parsed.Severity = NormalizeSeverity(parsed.Severity);
                parsed.Confidence = Math.Clamp(parsed.Confidence, 0, 100);
                parsed.Answer ??= "Please monitor symptoms and consult a doctor if they worsen.";

                return parsed;
            }
            catch (Exception ex)
            {
                return BuildFallback(request, $"Gemini analysis exception: {ex.Message}");
            }
        }

        private static string BuildInput(AiAnalyzeRequestDto request)
        {
            var parts = new List<string>();
            if (request.SelectedSymptoms?.Any() == true)
            {
                parts.Add($"Symptoms: {string.Join(", ", request.SelectedSymptoms)}");
            }

            if (!string.IsNullOrWhiteSpace(request.AdditionalDescription))
            {
                parts.Add($"Details: {request.AdditionalDescription.Trim()}");
            }

            if (!string.IsNullOrWhiteSpace(request.Question))
            {
                parts.Add($"Question: {request.Question.Trim()}");
            }

            return string.Join(" | ", parts);
        }

        private static string ExtractTextFromGemini(string responseBody)
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                return string.Empty;
            }

            var first = candidates[0];
            if (!first.TryGetProperty("content", out var content))
            {
                return string.Empty;
            }

            if (!content.TryGetProperty("parts", out var parts) || parts.GetArrayLength() == 0)
            {
                return string.Empty;
            }

            if (!parts[0].TryGetProperty("text", out var textElement))
            {
                return string.Empty;
            }

            return textElement.GetString() ?? string.Empty;
        }

        private static string? TryExtractJson(string text)
        {
            var start = text.IndexOf('{');
            var end = text.LastIndexOf('}');
            if (start < 0 || end <= start)
            {
                return null;
            }

            return text.Substring(start, end - start + 1);
        }

        private static string NormalizeSeverity(string? severity)
        {
            var value = (severity ?? string.Empty).Trim().ToLowerInvariant();
            return value switch
            {
                "high" or "severe" => "High",
                "moderate" or "medium" => "Moderate",
                _ => "Low"
            };
        }

        private static AiAnalyzeResultDto BuildFallback(AiAnalyzeRequestDto request, string answer)
        {
            var lowered = BuildInput(request).ToLowerInvariant();
            if (lowered.Contains("chest pain") || lowered.Contains("shortness of breath"))
            {
                return new AiAnalyzeResultDto
                {
                    Condition = "Possible urgent condition",
                    Confidence = 90,
                    Severity = "High",
                    Recommendations = new List<string>
                    {
                        "Seek immediate in-person medical evaluation.",
                        "Avoid self-medication until examined."
                    },
                    WarningSigns = new List<string>
                    {
                        "Severe chest pain",
                        "Difficulty breathing",
                        "Fainting"
                    },
                    Answer = answer
                };
            }

            return new AiAnalyzeResultDto
            {
                Condition = "General Health Guidance",
                Confidence = 65,
                Severity = "Low",
                Recommendations = new List<string>
                {
                    "Rest and hydrate.",
                    "Monitor symptoms for 24-48 hours.",
                    "Book a BUITEMS doctor appointment if symptoms persist."
                },
                WarningSigns = new List<string>
                {
                    "High persistent fever",
                    "Breathing difficulty",
                    "Worsening pain"
                },
                Answer = answer
            };
        }
    }
}
