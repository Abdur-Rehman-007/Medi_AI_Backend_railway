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
    public class NotificationsController : ControllerBase
    {
        private readonly MediaidbContext _context;

        public NotificationsController(MediaidbContext context)
        {
            _context = context;
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token" });
            }

            var userId = int.Parse(userIdClaim.Value);

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.IsRead != true)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.Type,
                    n.RelatedEntityId,
                    n.RelatedEntityType,
                    n.IsRead,
                    n.CreatedAt
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Unread notifications loaded",
                Data = notifications
            });
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token" });
            }

            var userId = int.Parse(userIdClaim.Value);
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = "Notification not found" });
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Notification marked as read",
                Data = new { notification.Id }
            });
        }

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token" });
            }

            var userId = int.Parse(userIdClaim.Value);

            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && n.IsRead != true)
                .ToListAsync();

            foreach (var item in unread)
            {
                item.IsRead = true;
                item.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "All notifications marked as read",
                Data = new { count = unread.Count }
            });
        }
    }
}
