using Backend_APIs.DTOs;
using Backend_APIs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace Backend_APIs.Controllers
{
    [Route("api/reminders")]
    [ApiController]
    [Authorize]
    public class RemindersController : ControllerBase
    {
        private readonly MediaidbContext _context;

        public RemindersController(MediaidbContext context)
        {
            _context = context;
        }

        [HttpPost("sync")]
        public async Task<IActionResult> Sync([FromBody] ReminderSyncRequestDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token" });
            }

            var userId = int.Parse(userIdClaim.Value);
            if (request.Reminders == null || request.Reminders.Count == 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "No reminders provided" });
            }

            var syncedIds = new List<int>();

            foreach (var item in request.Reminders)
            {
                if (string.IsNullOrWhiteSpace(item.MedicineName) || string.IsNullOrWhiteSpace(item.Dosage))
                {
                    continue;
                }

                var timesJson = JsonSerializer.Serialize(item.Times ?? new List<string>());

                Medicinereminder? entity = null;
                if (item.Id.HasValue)
                {
                    entity = await _context.Medicinereminders
                        .FirstOrDefaultAsync(r => r.Id == item.Id.Value && r.StudentId == userId);
                }

                if (entity == null)
                {
                    entity = new Medicinereminder
                    {
                        StudentId = userId,
                        MedicineName = item.MedicineName.Trim(),
                        Dosage = item.Dosage.Trim(),
                        Frequency = string.IsNullOrWhiteSpace(item.Frequency) ? "Custom" : item.Frequency.Trim(),
                        CustomFrequency = item.CustomFrequency,
                        Times = timesJson,
                        StartDate = item.StartDate ?? DateOnly.FromDateTime(DateTime.Today),
                        EndDate = item.EndDate,
                        Notes = item.Notes,
                        IsActive = item.IsActive ?? true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Medicinereminders.Add(entity);
                }
                else
                {
                    entity.MedicineName = item.MedicineName.Trim();
                    entity.Dosage = item.Dosage.Trim();
                    entity.Frequency = string.IsNullOrWhiteSpace(item.Frequency) ? entity.Frequency : item.Frequency.Trim();
                    entity.CustomFrequency = item.CustomFrequency;
                    entity.Times = timesJson;
                    entity.StartDate = item.StartDate ?? entity.StartDate;
                    entity.EndDate = item.EndDate;
                    entity.Notes = item.Notes;
                    entity.IsActive = item.IsActive ?? entity.IsActive;
                    entity.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                syncedIds.Add(entity.Id);
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Reminders synced successfully",
                Data = new { count = syncedIds.Count, reminderIds = syncedIds }
            });
        }
    }

    public class ReminderSyncRequestDto
    {
        public List<ReminderSyncItemDto> Reminders { get; set; } = new();
    }

    public class ReminderSyncItemDto
    {
        public int? Id { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string? Frequency { get; set; }
        public string? CustomFrequency { get; set; }
        public List<string>? Times { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Notes { get; set; }
        public bool? IsActive { get; set; }
    }
}
