using Backend_APIs.DTOs;
using Backend_APIs.Models;
using Backend_APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace Backend_APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SymptomCheckerController : ControllerBase
    {
        private readonly MediaidbContext _context;
        private readonly IGeminiAiService _geminiAiService;

        public SymptomCheckerController(MediaidbContext context, IGeminiAiService geminiAiService)
        {
            _context = context;
            _geminiAiService = geminiAiService;
        }

        [HttpPost("analyze")]
        public async Task<ActionResult<SymptomCheckResponseDto>> AnalyzeSymptoms([FromBody] AnalyzeSymptomsDto request, CancellationToken cancellationToken)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token", Data = null, Errors = null });
                var userId = int.Parse(userIdClaim.Value);

                // Call real AI service
                var aiRequest = new AiAnalyzeRequestDto
                {
                    SelectedSymptoms = request.SelectedSymptoms,
                    AdditionalDescription = request.AdditionalDescription,
                    Question = request.Question
                };

                var aiResult = await _geminiAiService.AnalyzeAsync(aiRequest, cancellationToken);

                var symptomCheck = new Symptomcheck
                {
                    UserId = userId,
                    Symptoms = JsonSerializer.Serialize(request.SelectedSymptoms),
                    Duration = "Unknown",
                    Severity = aiResult.Severity,
                    Airesponse = JsonSerializer.Serialize(new
                    {
                        aiResult.Condition,
                        aiResult.Answer,
                        aiResult.Recommendations,
                        aiResult.WarningSigns
                    }),
                    Confidence = (decimal?)aiResult.Confidence,
                    RecommendedAction = TruncateString(string.Join(", ", aiResult.Recommendations), 200),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Symptomchecks.Add(symptomCheck);
                await _context.SaveChangesAsync(cancellationToken);

                var response = new SymptomCheckResponseDto
                {
                    Id = symptomCheck.Id,
                    Symptoms = symptomCheck.Symptoms,
                    Condition = aiResult.Condition,
                    Confidence = (decimal)aiResult.Confidence,
                    Severity = aiResult.Severity,
                    Recommendations = aiResult.Recommendations,
                    WarningSigns = aiResult.WarningSigns,
                    Answer = aiResult.Answer,
                    CreatedAt = symptomCheck.CreatedAt ?? DateTime.UtcNow
                };

                return Ok(new ApiResponse<SymptomCheckResponseDto>
                {
                    Success = true,
                    Message = "Analysis complete",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Analysis failed: {ex.Message}"
                });
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<SymptomCheckResponseDto>>> GetHistory()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token", Data = null, Errors = null });
                var userId = int.Parse(userIdClaim.Value);

                var history = await _context.Symptomchecks
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();

                var response = history.Select(h =>
                {
                    var aiData = ParseFullAiResponse(h.Airesponse);

                    // Fallback for Recommendations if not in AIResponse JSON
                    var recommendations = aiData.Recommendations;
                    if (recommendations.Count == 0 && !string.IsNullOrWhiteSpace(h.RecommendedAction))
                    {
                        recommendations = h.RecommendedAction.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList();
                    }

                    return new SymptomCheckResponseDto
                    {
                        Id = h.Id,
                        Symptoms = ParseSymptoms(h.Symptoms),
                        Condition = aiData.Condition,
                        Answer = aiData.Answer,
                        Confidence = h.Confidence ?? 0,
                        Severity = h.Severity ?? "Unknown",
                        Recommendations = recommendations,
                        WarningSigns = aiData.WarningSigns,
                        CreatedAt = h.CreatedAt ?? DateTime.UtcNow
                    };
                });

                return Ok(new ApiResponse<IEnumerable<SymptomCheckResponseDto>>
                {
                    Success = true,
                    Message = "History retrieved",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to load history: {ex.Message}"
                });
            }
        }

        private static string TruncateString(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private static (string Condition, string Answer, List<string> Recommendations, List<string> WarningSigns) ParseFullAiResponse(string? json)
        {
            var result = (Condition: "Unknown", Answer: "No details available", Recommendations: new List<string>(), WarningSigns: new List<string>());

            if (string.IsNullOrWhiteSpace(json)) return result;

            try
            {
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("Condition", out var c)) result.Condition = c.GetString() ?? "Unknown";
                if (root.TryGetProperty("Answer", out var a)) result.Answer = a.GetString() ?? "No details available";

                if (root.TryGetProperty("Recommendations", out var recs) && recs.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in recs.EnumerateArray()) result.Recommendations.Add(item.GetString() ?? "");
                }

                if (root.TryGetProperty("WarningSigns", out var warns) && warns.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in warns.EnumerateArray()) result.WarningSigns.Add(item.GetString() ?? "");
                }

                return result;
            }
            catch
            {
                result.Condition = json ?? "Unknown";
                return result;
            }
        }

        private static string ParseSymptoms(string symptoms)
        {
            if (string.IsNullOrWhiteSpace(symptoms)) return "";
            if (symptoms.TrimStart().StartsWith("["))
            {
                try
                {
                    var list = JsonSerializer.Deserialize<List<string>>(symptoms);
                    return list != null ? string.Join(", ", list) : symptoms;
                }
                catch
                {
                    return symptoms;
                }
            }
            return symptoms;
        }
    }
}
