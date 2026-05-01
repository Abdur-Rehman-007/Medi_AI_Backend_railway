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
    public class EmergencyContactsController : ControllerBase
    {
        private readonly MediaidbContext _context;

        public EmergencyContactsController(MediaidbContext context)
        {
            _context = context;
        }

        // GET: api/EmergencyContacts
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<EmergencyContactDto>>>> GetMyContacts()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token", Data = null, Errors = null });
                var userId = int.Parse(userIdClaim.Value);

                var contacts = await _context.Emergencycontacts
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                var dtos = contacts.Select(c => new EmergencyContactDto
                {
                    Id = c.Id,
                    ContactName = c.ContactName,
                    Relationship = c.Relationship,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    Address = c.Address,
                    CreatedAt = c.CreatedAt ?? DateTime.UtcNow
                });

                return Ok(new ApiResponse<IEnumerable<EmergencyContactDto>>
                {
                    Success = true,
                    Message = "Emergency contacts retrieved",
                    Data = dtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        // POST: api/EmergencyContacts
        [HttpPost]
        public async Task<ActionResult<ApiResponse<EmergencyContactDto>>> AddContact(CreateEmergencyContactDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token", Data = null, Errors = null });
                var userId = int.Parse(userIdClaim.Value);

                var contact = new Emergencycontact
                {
                    UserId = userId,
                    ContactName = request.ContactName,
                    Relationship = request.Relationship,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    Address = request.Address,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Emergencycontacts.Add(contact);
                await _context.SaveChangesAsync();

                var dto = new EmergencyContactDto
                {
                    Id = contact.Id,
                    ContactName = contact.ContactName,
                    Relationship = contact.Relationship,
                    PhoneNumber = contact.PhoneNumber,
                    Email = contact.Email,
                    Address = contact.Address,
                    CreatedAt = contact.CreatedAt ?? DateTime.UtcNow
                };

                return CreatedAtAction(nameof(GetMyContacts), new { id = contact.Id }, new ApiResponse<EmergencyContactDto>
                {
                    Success = true,
                    Message = "Contact added",
                    Data = dto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error adding contact: {ex.Message}"
                });
            }
        }

        // DELETE: api/EmergencyContacts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContact(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token", Data = null, Errors = null });
                var userId = int.Parse(userIdClaim.Value);

                var contact = await _context.Emergencycontacts
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                if (contact == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Contact not found"
                    });
                }

                _context.Emergencycontacts.Remove(contact);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Contact deleted"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error deleting contact: {ex.Message}"
                });
            }
        }
    }
}
