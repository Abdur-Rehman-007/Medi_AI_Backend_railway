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
    public class HealthTipsController : ControllerBase
    {
        private readonly MediaidbContext _context;

        public HealthTipsController(MediaidbContext context)
        {
            _context = context;
        }

        // GET: api/HealthTips
        [HttpGet]
        [AllowAnonymous] // Assuming guests might see this, or restrict to Authenticated
        public async Task<ActionResult<IEnumerable<HealthTipResponseDto>>> GetHealthTips([FromQuery] string? category)
        {
            var query = _context.Healthtips
                .Include(h => h.Author)
                .Where(h => h.IsPublished == true);

            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                query = query.Where(h => h.Category == category);
            }

            var tips = await query
                .OrderByDescending(h => h.PublishedAt)
                .Select(h => new HealthTipResponseDto
                {
                    Id = h.Id,
                    Title = h.Title,
                    Category = h.Category,
                    Content = h.Content,
                    ImageUrl = h.ImageUrl,
                    Source = h.Source,
                    AuthorName = h.Author != null ? h.Author.FullName : "System",
                    Views = h.Views ?? 0,
                    Likes = h.Likes ?? 0,
                    PublishedAt = h.PublishedAt ?? h.CreatedAt ?? DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<HealthTipResponseDto>>
            {
                Success = true,
                Message = "Health tips retrieved successfully",
                Data = tips
            });
        }

        // GET: api/HealthTips/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HealthTipResponseDto>> GetHealthTip(int id)
        {
            var healthTip = await _context.Healthtips
                .Include(h => h.Author)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (healthTip == null)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = "Health tip not found" });
            }

            // Increment views
            healthTip.Views = (healthTip.Views ?? 0) + 1;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<HealthTipResponseDto>
            {
                Success = true,
                Message = "Health tip retrieved successfully",
                Data = new HealthTipResponseDto
                {
                    Id = healthTip.Id,
                    Title = healthTip.Title,
                    Category = healthTip.Category,
                    Content = healthTip.Content,
                    ImageUrl = healthTip.ImageUrl,
                    Source = healthTip.Source,
                    AuthorName = healthTip.Author != null ? healthTip.Author.FullName : "System",
                    Views = healthTip.Views ?? 0,
                    Likes = healthTip.Likes ?? 0,
                    PublishedAt = healthTip.PublishedAt ?? healthTip.CreatedAt ?? DateTime.UtcNow
                }
            });
        }

        // POST: api/HealthTips (Admin/Doctor/Faculty only)
        [HttpPost]
        [Authorize(Roles = "admin,Admin,doctor,Doctor,faculty,Faculty")]
        public async Task<ActionResult<HealthTipResponseDto>> CreateHealthTip(CreateHealthTipDto createDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;

            var healthTip = new Healthtip
            {
                Title = createDto.Title,
                Category = createDto.Category,
                Content = createDto.Content,
                ImageUrl = createDto.ImageUrl,
                Source = createDto.Source,
                AuthorId = userId,
                Views = 0,
                Likes = 0,
                IsPublished = createDto.IsPublished,
                CreatedAt = DateTime.UtcNow,
                PublishedAt = createDto.IsPublished ? DateTime.UtcNow : null
            };

            _context.Healthtips.Add(healthTip);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHealthTip), new { id = healthTip.Id }, new ApiResponse<object>
            {
                Success = true,
                Message = "Health tip created successfully",
                Data = new { healthTip.Id }
            });
        }

        // PUT: api/HealthTips/5
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,Admin,doctor,Doctor,faculty,Faculty")]
        public async Task<IActionResult> UpdateHealthTip(int id, UpdateHealthTipDto updateDto)
        {
            var healthTip = await _context.Healthtips.FindAsync(id);
            if (healthTip == null)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = "Health tip not found" });
            }

            if (updateDto.Title != null) healthTip.Title = updateDto.Title;
            if (updateDto.Category != null) healthTip.Category = updateDto.Category;
            if (updateDto.Content != null) healthTip.Content = updateDto.Content;
            if (updateDto.ImageUrl != null) healthTip.ImageUrl = updateDto.ImageUrl;
            if (updateDto.Source != null) healthTip.Source = updateDto.Source;
            if (updateDto.IsPublished.HasValue)
            {
                healthTip.IsPublished = updateDto.IsPublished.Value;
                if (healthTip.IsPublished == true && healthTip.PublishedAt == null)
                {
                    healthTip.PublishedAt = DateTime.UtcNow;
                }
            }

            healthTip.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object> { Success = true, Message = "Health tip updated successfully" });
        }

        // DELETE: api/HealthTips/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> DeleteHealthTip(int id)
        {
            var healthTip = await _context.Healthtips.FindAsync(id);
            if (healthTip == null)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = "Health tip not found" });
            }

            _context.Healthtips.Remove(healthTip);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object> { Success = true, Message = "Health tip deleted successfully" });
        }

        // POST: api/HealthTips/5/like
        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikeHealthTip(int id)
        {
            var healthTip = await _context.Healthtips.FindAsync(id);
            if (healthTip == null)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = "Health tip not found" });
            }

            // Simple like increment. Robust solution would track user likes in Healthtipinteraction table
            healthTip.Likes = (healthTip.Likes ?? 0) + 1;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Health tip liked",
                Data = new { likes = healthTip.Likes }
            });
        }
    }
}
