using Backend_APIs.DTOs;
using Backend_APIs.Models;
using Backend_APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace Backend_APIs.Controllers
{
    [Route("api/ai")]
    [ApiController]
    [Authorize]
    public class AiController : ControllerBase
    {
        private readonly IGeminiAiService _geminiAiService;
        private readonly MediaidbContext _context;

        public AiController(IGeminiAiService geminiAiService, MediaidbContext context)
        {
            _geminiAiService = geminiAiService;
            _context = context;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze([FromBody] AiAnalyzeRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token" });
                }

                var userId = int.Parse(userIdClaim.Value);
                var result = await _geminiAiService.AnalyzeAsync(request, cancellationToken);

                var symptoms = new List<string>(request.SelectedSymptoms ?? new List<string>());
                if (!string.IsNullOrWhiteSpace(request.AdditionalDescription))
                {
                    symptoms.Add(request.AdditionalDescription.Trim());
                }

                var record = new Symptomcheck
                {
                    UserId = userId,
                    Symptoms = JsonSerializer.Serialize(symptoms),
                    Duration = "Unknown",
                    Severity = result.Severity,
                    Airesponse = JsonSerializer.Serialize(new
                    {
                        result.Condition,
                        result.Answer
                    }),
                    Confidence = (decimal?)result.Confidence,
                    RecommendedAction = JsonSerializer.Serialize(new
                    {
                        result.Recommendations,
                        result.WarningSigns
                    }),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Symptomchecks.Add(record);
                await _context.SaveChangesAsync(cancellationToken);

                return Ok(new ApiResponse<AiAnalyzeResultDto>
                {
                    Success = true,
                    Message = "AI analysis complete",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"AI analysis failed: {ex.Message}",
                    Data = null
                });
            }
        }
    }
}
