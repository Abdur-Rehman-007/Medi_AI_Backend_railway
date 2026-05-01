using Backend_APIs.DTOs;
using Backend_APIs.Models;
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

        public SymptomCheckerController(MediaidbContext context)
        {
            _context = context;
        }

        [HttpPost("analyze")]
        public async Task<ActionResult<SymptomCheckResponseDto>> AnalyzeSymptoms([FromBody] AnalyzeSymptomsDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token", Data = null, Errors = null });
                var userId = int.Parse(userIdClaim.Value);

                // combine symptoms for storage
                var allSymptomsList = new List<string>(request.SelectedSymptoms);
                if (!string.IsNullOrWhiteSpace(request.AdditionalDescription))
                {
                    allSymptomsList.Add(request.AdditionalDescription);
                }
                var symptomsString = string.Join(", ", allSymptomsList);

                // --- MOCK AI LOGIC (Replace with real Python/AI Service call later) ---
                var mockResult = MockAIAnalysis(allSymptomsList);
                // ---------------------------------------------------------------------

                var symptomCheck = new Symptomcheck
                {
                    UserId = userId,
                    Symptoms = symptomsString,
                    Duration = "Unknown", // Frontend could provide this
                    Severity = mockResult.Severity,
                    Airesponse = mockResult.Condition, // Storing main condition in Airesponse
                    Confidence = mockResult.Confidence,
                    RecommendedAction = JsonSerializer.Serialize(new
                    {
                        Recommendations = mockResult.Recommendations,
                        Warnings = mockResult.WarningSigns
                    }),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Symptomchecks.Add(symptomCheck);
                await _context.SaveChangesAsync();

                mockResult.Id = symptomCheck.Id;
                mockResult.Symptoms = symptomsString;
                mockResult.CreatedAt = symptomCheck.CreatedAt ?? DateTime.UtcNow;

                return Ok(new ApiResponse<SymptomCheckResponseDto>
                {
                    Success = true,
                    Message = "Analysis complete",
                    Data = mockResult
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
                    var actions = ParseActions(h.RecommendedAction);
                    return new SymptomCheckResponseDto
                    {
                        Id = h.Id,
                        Symptoms = h.Symptoms,
                        Condition = h.Airesponse ?? "Unknown",
                        Confidence = h.Confidence ?? 0,
                        Severity = h.Severity ?? "Unknown",
                        Recommendations = actions.Recommendations,
                        WarningSigns = actions.Warnings,
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

        private SymptomCheckResponseDto MockAIAnalysis(List<string> symptoms)
        {
            // Simple keyword matching mock
            var s = string.Join(" ", symptoms).ToLower();

            if (s.Contains("chest pain") || s.Contains("shortness of breath"))
            {
                return new SymptomCheckResponseDto
                {
                    Condition = "Potential Cardiac Issue",
                    Confidence = 95,
                    Severity = "High",
                    Recommendations = new List<string> { "Seek immediate medical attention", "Do not drive yourself" },
                    WarningSigns = new List<string> { "Loss of consciousness", "Crushing chest pain" }
                };
            }
            if (s.Contains("fever") && s.Contains("cough"))
            {
                return new SymptomCheckResponseDto
                {
                    Condition = "Viral Infection / Flu",
                    Confidence = 85,
                    Severity = "Moderate",
                    Recommendations = new List<string> { "Rest", "Hydration", "Monitor temperature" },
                    WarningSigns = new List<string> { "Difficulty breathing", "High fever > 103F" }
                };
            }

            return new SymptomCheckResponseDto
            {
                Condition = "General Malaise",
                Confidence = 70,
                Severity = "Low",
                Recommendations = new List<string> { "Rest", "Monitor symptoms" },
                WarningSigns = new List<string> { "Symptoms worsen" }
            };
        }

        private (List<string> Recommendations, List<string> Warnings) ParseActions(string? json)
        {
            if (string.IsNullOrEmpty(json)) return (new List<string>(), new List<string>());
            try
            {
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var recs = new List<string>();
                if (root.TryGetProperty("Recommendations", out var recProp))
                {
                    foreach (var item in recProp.EnumerateArray()) recs.Add(item.GetString() ?? "");
                }

                var warns = new List<string>();
                if (root.TryGetProperty("Warnings", out var warnProp))
                {
                    foreach (var item in warnProp.EnumerateArray()) warns.Add(item.GetString() ?? "");
                }

                return (recs, warns);
            }
            catch
            {
                return (new List<string> { json }, new List<string>());
            }
        }
    }
}
