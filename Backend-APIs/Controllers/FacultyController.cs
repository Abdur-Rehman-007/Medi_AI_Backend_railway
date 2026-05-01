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
    public class FacultyController : ControllerBase
    {
        private readonly MediaidbContext _context;

        public FacultyController(MediaidbContext context)
        {
            _context = context;
        }

        [HttpGet("appointments")]
        public async Task<IActionResult> GetAppointments()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                var userId = int.Parse(userIdClaim.Value);

                var appointments = await _context.Appointments
                    .Include(a => a.Doctor)
                        .ThenInclude(d => d.User)
                    .Where(a => a.PatientId == userId)
                    .OrderByDescending(a => a.AppointmentDate)
                    .Select(a => new AppointmentResponseDto
                    {
                        Id = a.Id.ToString(),
                        PatientId = a.PatientId.ToString(),
                        // PatientName unnecessary for self
                        DoctorId = a.DoctorId.ToString(),
                        DoctorName = a.Doctor.User.FullName,
                        Specialization = a.Doctor.Specialization,
                        DateTime = a.AppointmentDate.ToString("yyyy-MM-dd") + "T" + a.AppointmentTime.ToString("HH:mm:ss"),
                        Status = a.Status,
                        Symptoms = a.Symptoms,
                        Notes = a.Notes,
                        CreatedAt = a.CreatedAt.ToString()
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<IEnumerable<AppointmentResponseDto>>
                {
                    Success = true,
                    Message = "Appointments loaded",
                    Data = appointments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                var userId = int.Parse(userIdClaim.Value);

                var totalAppointments = await _context.Appointments.CountAsync(a => a.PatientId == userId);
                var completedAppointments = await _context.Appointments.CountAsync(a => a.PatientId == userId && a.Status == "Completed");
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Statistics loaded",
                    Data = new
                    {
                        totalAppointments,
                        completedAppointments
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
            }
        }
    }
}
