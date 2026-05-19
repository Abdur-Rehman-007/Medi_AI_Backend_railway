using Backend_APIs.DTOs;
using Backend_APIs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend_APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FeedbackController : ControllerBase
    {
        private readonly MediaidbContext _context;

        public FeedbackController(MediaidbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFeedback([FromBody] SubmitFeedbackRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Data = ModelState
                });
            }

            var userIdResult = GetCurrentUserId();
            if (!userIdResult.Success)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = userIdResult.Message,
                    Data = null
                });
            }

            var userId = userIdResult.UserId!.Value;
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (currentUser == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found",
                    Data = null
                });
            }

            var feedback = new Feedback
            {
                UserId = userId,
                Subject = request.Subject.Trim(),
                Message = request.Message.Trim(),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            var adminUsers = await _context.Users
                .Where(u => u.Role == "Admin" || u.Role == "admin")
                .Select(u => new { u.Id, u.FullName })
                .ToListAsync();

            foreach (var admin in adminUsers)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = admin.Id,
                    Title = "New Feedback Submitted",
                    Message = $"{currentUser.FullName} submitted feedback: {feedback.Subject}",
                    Type = "General",
                    RelatedEntityId = feedback.Id,
                    RelatedEntityType = "Feedback",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (adminUsers.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Feedback submitted successfully",
                Data = new
                {
                    feedback.Id,
                    feedback.Subject,
                    feedback.Status,
                    feedback.CreatedAt
                }
            });
        }

        [HttpGet("my-feedback")]
        public async Task<IActionResult> GetMyFeedback()
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.Success)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = userIdResult.Message,
                    Data = null
                });
            }

            var userId = userIdResult.UserId!.Value;

            var feedback = await _context.Feedbacks
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new
                {
                    f.Id,
                    f.Subject,
                    f.Message,
                    f.AdminResponse,
                    f.Status,
                    f.CreatedAt,
                    f.RespondedAt
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Feedback history loaded successfully",
                Data = feedback
            });
        }

        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllFeedback()
        {
            var feedback = await _context.Feedbacks
                .AsNoTracking()
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new
                {
                    f.Id,
                    f.Subject,
                    f.Message,
                    f.AdminResponse,
                    f.Status,
                    f.CreatedAt,
                    f.RespondedAt,
                    User = new
                    {
                        f.User.Id,
                        Name = f.User.FullName,
                        f.User.Email,
                        f.User.Role
                    }
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Feedback loaded successfully",
                Data = feedback
            });
        }

        [HttpPut("admin/{id}/respond")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RespondToFeedback(int id, [FromBody] RespondFeedbackRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Data = ModelState
                });
            }

            var feedback = await _context.Feedbacks
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (feedback == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Feedback not found",
                    Data = null
                });
            }

            feedback.AdminResponse = request.AdminResponse.Trim();
            feedback.Status = "Responded";
            feedback.RespondedAt = DateTime.UtcNow;

            _context.Notifications.Add(new Notification
            {
                UserId = feedback.UserId,
                Title = "Feedback Response Received",
                Message = $"Your feedback '{feedback.Subject}' has received a reply.",
                Type = "General",
                RelatedEntityId = feedback.Id,
                RelatedEntityType = "Feedback",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Feedback response saved successfully",
                Data = new
                {
                    feedback.Id,
                    feedback.Status,
                    feedback.RespondedAt
                }
            });
        }

        private (bool Success, int? UserId, string Message) GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return (false, null, "Invalid token");
            }

            return (true, userId, string.Empty);
        }

        public class SubmitFeedbackRequest
        {
            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.MaxLength(200)]
            public string Subject { get; set; } = string.Empty;

            [System.ComponentModel.DataAnnotations.Required]
            public string Message { get; set; } = string.Empty;
        }

        public class RespondFeedbackRequest
        {
            [System.ComponentModel.DataAnnotations.Required]
            public string AdminResponse { get; set; } = string.Empty;
        }
    }
}