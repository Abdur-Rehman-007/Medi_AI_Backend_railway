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
    public class MedicalHistoryController : ControllerBase
    {
        private readonly MediaidbContext _context;

        public MedicalHistoryController(MediaidbContext context)
        {
            _context = context;
        }

        // GET: api/MedicalHistory
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<MedicalHistoryDto>>>> GetMyMedicalHistory()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token", Data = null, Errors = null });
                var userId = int.Parse(userIdClaim.Value);

                var history = await _context.Medicalhistories
                    .Where(m => m.PatientId == userId)
                    .OrderByDescending(m => m.DiagnosisDate)
                    .ThenByDescending(m => m.CreatedAt)
                    .ToListAsync();

                var dtos = history.Select(m => new MedicalHistoryDto
                {
                    Id = m.Id,
                    RecordType = m.RecordType,
                    Title = m.Title,
                    Description = m.Description,
                    DiagnosisDate = m.DiagnosisDate?.ToString("yyyy-MM-dd"),
                    Notes = m.Notes,
                    CreatedAt = m.CreatedAt ?? DateTime.UtcNow
                });

                return Ok(new ApiResponse<IEnumerable<MedicalHistoryDto>>
                {
                    Success = true,
                    Message = "Medical history retrieved successfully",
                    Data = dtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error retrieving medical history: {ex.Message}"
                });
            }
        }

        // GET: api/MedicalHistory/patient/{patientId}
        [HttpGet("patient/{patientId:int}")]
        [Authorize(Roles = "doctor,Doctor,admin,Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<MedicalHistoryDto>>>> GetPatientMedicalHistory(int patientId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid token",
                        Data = null,
                        Errors = null
                    });
                }

                var userId = int.Parse(userIdClaim.Value);
                var isAdmin = User.IsInRole("admin") || User.IsInRole("Admin");

                if (!isAdmin)
                {
                    var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                    if (doctor == null)
                    {
                        return NotFound(new ApiResponse<object>
                        {
                            Success = false,
                            Message = "Doctor profile not found"
                        });
                    }

                    var hasAccess = await _context.Appointments.AnyAsync(a =>
                        a.DoctorId == doctor.Id && a.PatientId == patientId);

                    if (!hasAccess)
                    {
                        return Forbid();
                    }
                }

                var history = await _context.Medicalhistories
                    .Where(m => m.PatientId == patientId)
                    .OrderByDescending(m => m.DiagnosisDate)
                    .ThenByDescending(m => m.CreatedAt)
                    .ToListAsync();

                var dtos = history.Select(m => new MedicalHistoryDto
                {
                    Id = m.Id,
                    RecordType = m.RecordType,
                    Title = m.Title,
                    Description = m.Description,
                    DiagnosisDate = m.DiagnosisDate?.ToString("yyyy-MM-dd"),
                    Notes = m.Notes,
                    CreatedAt = m.CreatedAt ?? DateTime.UtcNow
                });

                return Ok(new ApiResponse<IEnumerable<MedicalHistoryDto>>
                {
                    Success = true,
                    Message = "Patient medical history retrieved successfully",
                    Data = dtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error retrieving patient medical history: {ex.Message}"
                });
            }
        }

        // POST: api/MedicalHistory
        [HttpPost]
        public async Task<ActionResult<ApiResponse<MedicalHistoryDto>>> CreateMedicalHistory(CreateMedicalHistoryDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token", Data = null, Errors = null });
                var userId = int.Parse(userIdClaim.Value);

                var medicalHistory = new Medicalhistory
                {
                    PatientId = userId,
                    RecordType = request.RecordType,
                    Title = request.Title,
                    Description = request.Description,
                    DiagnosisDate = request.DiagnosisDate.HasValue ? DateOnly.FromDateTime(request.DiagnosisDate.Value) : null,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Medicalhistories.Add(medicalHistory);
                await _context.SaveChangesAsync();

                var dto = new MedicalHistoryDto
                {
                    Id = medicalHistory.Id,
                    RecordType = medicalHistory.RecordType,
                    Title = medicalHistory.Title,
                    Description = medicalHistory.Description,
                    DiagnosisDate = medicalHistory.DiagnosisDate?.ToString("yyyy-MM-dd"),
                    Notes = medicalHistory.Notes,
                    CreatedAt = medicalHistory.CreatedAt ?? DateTime.UtcNow
                };

                return CreatedAtAction(nameof(GetMyMedicalHistory), new { id = medicalHistory.Id }, new ApiResponse<MedicalHistoryDto>
                {
                    Success = true,
                    Message = "Medical history record created",
                    Data = dto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error creating record: {ex.Message}"
                });
            }
        }

        // DELETE: api/MedicalHistory/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedicalHistory(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token", Data = null, Errors = null });
                var userId = int.Parse(userIdClaim.Value);

                var record = await _context.Medicalhistories
                    .FirstOrDefaultAsync(m => m.Id == id && m.PatientId == userId);

                if (record == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Record not found or access denied"
                    });
                }

                _context.Medicalhistories.Remove(record);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Record deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error deleting record: {ex.Message}"
                });
            }
        }
    }
}
