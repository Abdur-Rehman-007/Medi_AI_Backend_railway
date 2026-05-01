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
    public class AppointmentsController : ControllerBase
    {
        private readonly MediaidbContext _context;
        private const string BookingSettingsPrefix = "DoctorBookingSettings:";
        private static readonly HashSet<int> AllowedSlotDurations = new() { 15, 20, 30, 45, 60 };
        private static readonly TimeOnly UniversityStartTime = new(8, 0);
        private static readonly TimeOnly UniversityEndTime = new(17, 0);
        private static readonly HashSet<DayOfWeek> UniversityWorkingDays = new()
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday
        };

        public AppointmentsController(MediaidbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all appointments (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> GetAllAppointments()
        {
            try
            {
                var appointmentList = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                        .ThenInclude(d => d.User)
                    .Include(a => a.Prescriptions)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.AppointmentTime)
                    .ToListAsync();

                var appointments = appointmentList.Select(a => new AppointmentResponseDto
                {
                    Id = a.Id.ToString(),
                    PatientId = a.PatientId.ToString(),
                    PatientName = a.Patient?.FullName ?? "Unknown",
                    DoctorId = a.DoctorId.ToString(),
                    DoctorName = a.Doctor?.User?.FullName ?? "Unknown",
                    Specialization = a.Doctor?.Specialization ?? "Unknown",
                    DateTime = new DateTime(a.AppointmentDate.Year, a.AppointmentDate.Month, a.AppointmentDate.Day,
                                          a.AppointmentTime.Hour, a.AppointmentTime.Minute, a.AppointmentTime.Second).ToString("o"),
                    Status = a.Status,
                    Symptoms = a.Symptoms,
                    Notes = a.Notes,
                    Prescription = a.Prescriptions.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.Notes,
                    CreatedAt = a.CreatedAt.HasValue ? a.CreatedAt.Value.ToString("o") : null
                }).ToList();

                return Ok(new ApiResponse<List<AppointmentResponseDto>>
                {
                    Success = true,
                    Message = "All appointments retrieved successfully",
                    Data = appointments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to retrieve appointments: {ex.Message}",
                    Data = null
                });
            }
        }
        /// <summary>
        /// Get student/patient appointment history
        /// </summary>
        [HttpGet("student/{studentId}/history")]
        public async Task<IActionResult> GetStudentAppointmentHistory(string studentId)
        {
            try
            {
                // Verify requesting user
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid token",
                        Data = null
                    });
                }

                if (!int.TryParse(studentId, out int patientId))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid student ID",
                        Data = null
                    });
                }

                var today = DateOnly.FromDateTime(DateTime.Today);

                var appointmentList = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                        .ThenInclude(d => d.User)
                    .Include(a => a.Prescriptions)
                    .Where(a => a.PatientId == patientId) // Get all, then separate by date/status? OR just get past
                    .Where(a => a.AppointmentDate < today || a.Status == "Completed" || a.Status == "Cancelled")
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.AppointmentTime)
                    .ToListAsync();

                var appointments = appointmentList.Select(a => new AppointmentResponseDto
                {
                    Id = a.Id.ToString(),
                    PatientId = a.PatientId.ToString(),
                    PatientName = a.Patient.FullName,
                    DoctorId = a.DoctorId.ToString(),
                    DoctorName = a.Doctor.User.FullName,
                    Specialization = a.Doctor.Specialization,
                    DateTime = new DateTime(a.AppointmentDate.Year, a.AppointmentDate.Month, a.AppointmentDate.Day,
                                          a.AppointmentTime.Hour, a.AppointmentTime.Minute, a.AppointmentTime.Second).ToString("o"),
                    Status = a.Status,
                    Symptoms = a.Symptoms,
                    Notes = a.Notes,
                    Prescription = a.Prescriptions.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.Notes,
                    CreatedAt = a.CreatedAt.HasValue ? a.CreatedAt.Value.ToString("o") : null
                }).ToList();

                return Ok(new ApiResponse<List<AppointmentResponseDto>>
                {
                    Success = true,
                    Message = "Appointment history retrieved successfully",
                    Data = appointments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to retrieve appointment history: {ex.Message}",
                    Data = null
                });
            }
        }
        /// <summary>
        /// Book a new appointment
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BookAppointment([FromBody] CreateAppointmentDto appointmentDto)
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
                        Data = null
                    });
                }

                var userId = int.Parse(userIdClaim.Value);

                // Parse doctor ID
                if (!int.TryParse(appointmentDto.DoctorId, out int doctorId))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid doctor ID",
                        Data = null
                    });
                }

                // Validate doctor exists
                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.Id == doctorId);

                if (doctor == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Doctor not found",
                        Data = null
                    });
                }

                // Check if doctor is available
                if (doctor.IsAvailable == false)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Doctor is not available",
                        Data = null
                    });
                }

                // Resolve doctor booking settings from DB and validate selected slot
                var appointmentDateTime = DateTime.Parse(appointmentDto.DateTime);
                var appointmentDate = DateOnly.FromDateTime(appointmentDateTime);
                var appointmentTime = TimeOnly.FromDateTime(appointmentDateTime);
                var bookingSettings = await GetDoctorBookingSettingsAsync(doctorId);

                if (!UniversityWorkingDays.Contains(appointmentDateTime.DayOfWeek))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Appointments are only allowed on university working days (Monday-Friday)",
                        Data = null
                    });
                }

                if (appointmentTime < UniversityStartTime || appointmentTime >= UniversityEndTime)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Appointments must be within university timings (08:00 - 17:00)",
                        Data = null
                    });
                }

                var dayName = appointmentDateTime.DayOfWeek.ToString();
                var activeSchedule = await _context.Doctorschedules
                    .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.DayOfWeek == dayName && s.IsActive == true);

                if (activeSchedule == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Doctor is not available on this day",
                        Data = null
                    });
                }

                if (appointmentTime < activeSchedule.StartTime || appointmentTime >= activeSchedule.EndTime)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Selected time is outside doctor's schedule",
                        Data = null
                    });
                }

                if (bookingSettings.EnableBreakTime
                    && TimeOnly.TryParse(bookingSettings.BreakStartTime, out var breakStart)
                    && TimeOnly.TryParse(bookingSettings.BreakEndTime, out var breakEnd)
                    && breakStart < breakEnd
                    && appointmentTime >= breakStart
                    && appointmentTime < breakEnd)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Selected time is in doctor's break period",
                        Data = null
                    });
                }

                var minutesFromStart = (int)(appointmentTime.ToTimeSpan() - activeSchedule.StartTime.ToTimeSpan()).TotalMinutes;
                if (minutesFromStart < 0 || minutesFromStart % bookingSettings.AppointmentDuration != 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Please select an exact available slot",
                        Data = null
                    });
                }

                var totalAppointmentsToday = await _context.Appointments
                    .CountAsync(a => a.DoctorId == doctorId
                                     && a.AppointmentDate == appointmentDate
                                     && a.Status != "Cancelled");

                if (totalAppointmentsToday >= bookingSettings.MaxPatientsPerDay)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Doctor has reached maximum appointments for this day",
                        Data = null
                    });
                }

                var conflictingAppointment = await _context.Appointments
                    .Where(a => a.DoctorId == doctorId
                        && a.AppointmentDate == appointmentDate
                        && a.AppointmentTime == appointmentTime
                        && a.Status != "Cancelled")
                    .FirstOrDefaultAsync();

                if (conflictingAppointment != null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "This time slot is already booked",
                        Data = null
                    });
                }

                // Get patient info
                var patient = await _context.Users.FindAsync(userId);
                if (patient == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Patient not found",
                        Data = null
                    });
                }

                // Create appointment
                var appointment = new Appointment
                {
                    PatientId = userId,
                    DoctorId = doctorId,
                    AppointmentDate = appointmentDate,
                    AppointmentTime = appointmentTime,
                    Duration = bookingSettings.AppointmentDuration,
                    Status = bookingSettings.AutoConfirmAppointments ? "Confirmed" : "Pending",
                    Symptoms = appointmentDto.Symptoms,
                    Notes = appointmentDto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                _context.Notifications.Add(new Notification
                {
                    UserId = doctor.UserId,
                    Title = "New Appointment Booking",
                    Message = $"{patient.FullName} booked an appointment on {appointmentDate:yyyy-MM-dd} at {appointmentTime:HH\\:mm}",
                    Type = "Appointment",
                    RelatedEntityId = appointment.Id,
                    RelatedEntityType = "Appointment",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                var responseData = new AppointmentResponseDto
                {
                    Id = appointment.Id.ToString(),
                    PatientId = userId.ToString(),
                    PatientName = patient.FullName,
                    DoctorId = doctor.Id.ToString(),
                    DoctorName = doctor.User.FullName,
                    Specialization = doctor.Specialization,
                    DateTime = appointmentDateTime.ToString("o"),
                    Status = appointment.Status,
                    Symptoms = appointment.Symptoms,
                    Notes = appointment.Notes,
                    Prescription = null,
                    CreatedAt = appointment.CreatedAt.HasValue ? appointment.CreatedAt.Value.ToString("o") : null
                };

                return Ok(new ApiResponse<AppointmentResponseDto>
                {
                    Success = true,
                    Message = "Appointment booked successfully",
                    Data = responseData
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to book appointment: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get my appointments (Student or Doctor based on token)
        /// </summary>
        [HttpGet("my-appointments")]
        public async Task<IActionResult> GetMyAppointments()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token" });

                var roleClaim = User.FindFirst(ClaimTypes.Role);
                if (roleClaim == null) return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token" });

                var userId = int.Parse(userIdClaim.Value);
                var role = roleClaim.Value.ToLower();

                List<Appointment> appointmentsList;

                if (role == "doctor")
                {
                    var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                    if (doctor == null) return NotFound(new ApiResponse<object> { Success = false, Message = "Doctor profile not found" });

                    appointmentsList = await _context.Appointments
                        .Include(a => a.Patient)
                        .Include(a => a.Prescriptions)
                        .Where(a => a.DoctorId == doctor.Id)
                        .OrderByDescending(a => a.AppointmentDate)
                        .ThenByDescending(a => a.AppointmentTime)
                        .ToListAsync();
                }
                else // student/patient
                {
                    appointmentsList = await _context.Appointments
                        .Include(a => a.Doctor).ThenInclude(d => d.User)
                        .Include(a => a.Prescriptions)
                        .Where(a => a.PatientId == userId)
                        .OrderByDescending(a => a.AppointmentDate)
                        .ThenByDescending(a => a.AppointmentTime)
                        .ToListAsync();
                }

                var response = appointmentsList.Select(a => new AppointmentResponseDto
                {
                    Id = a.Id.ToString(),
                    PatientId = a.PatientId.ToString(),
                    PatientName = a.Patient?.FullName ?? "Unknown",
                    DoctorId = a.DoctorId.ToString(),
                    DoctorName = a.Doctor?.User?.FullName ?? "Unknown",
                    Specialization = a.Doctor?.Specialization ?? "Unknown",
                    DateTime = new DateTime(a.AppointmentDate.Year, a.AppointmentDate.Month, a.AppointmentDate.Day,
                                          a.AppointmentTime.Hour, a.AppointmentTime.Minute, a.AppointmentTime.Second).ToString("o"),
                    Status = a.Status,
                    Symptoms = a.Symptoms,
                    Notes = a.Notes,
                    Prescription = a.Prescriptions.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.Notes,
                    CreatedAt = a.CreatedAt.HasValue ? a.CreatedAt.Value.ToString("o") : null
                }).ToList();

                return Ok(new ApiResponse<List<AppointmentResponseDto>>
                {
                    Success = true,
                    Message = "Appointments retrieved successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to retrieve appointments: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get student/patient upcoming appointments
        /// </summary>
        [HttpGet("student/{studentId}/upcoming")]
        public async Task<IActionResult> GetStudentUpcomingAppointments(string studentId)
        {
            try
            {
                // Verify requesting user
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid token",
                        Data = null
                    });
                }

                if (!int.TryParse(studentId, out int patientId))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid student ID",
                        Data = null
                    });
                }

                var today = DateOnly.FromDateTime(DateTime.Today);

                var appointmentList = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                        .ThenInclude(d => d.User)
                    .Include(a => a.Prescriptions)
                    .Where(a => a.PatientId == patientId && a.AppointmentDate >= today)
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentTime)
                    .ToListAsync();

                var appointments = appointmentList.Select(a => new AppointmentResponseDto
                {
                    Id = a.Id.ToString(),
                    PatientId = a.PatientId.ToString(),
                    PatientName = a.Patient.FullName,
                    DoctorId = a.DoctorId.ToString(),
                    DoctorName = a.Doctor.User.FullName,
                    Specialization = a.Doctor.Specialization,
                    DateTime = new DateTime(a.AppointmentDate.Year, a.AppointmentDate.Month, a.AppointmentDate.Day,
                                          a.AppointmentTime.Hour, a.AppointmentTime.Minute, a.AppointmentTime.Second).ToString("o"),
                    Status = a.Status,
                    Symptoms = a.Symptoms,
                    Notes = a.Notes,
                    Prescription = a.Prescriptions.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.Notes,
                    CreatedAt = a.CreatedAt.HasValue ? a.CreatedAt.Value.ToString("o") : null
                }).ToList();

                return Ok(new ApiResponse<List<AppointmentResponseDto>>
                {
                    Success = true,
                    Message = "Appointments retrieved successfully",
                    Data = appointments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to retrieve appointments: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get faculty/doctor appointments
        /// </summary>
        [HttpGet("Faculty/appointments")]
        [Authorize(Roles = "doctor,Faculty,admin")]
        public async Task<IActionResult> GetFacultyAppointments()
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
                        Data = null
                    });
                }

                var userId = int.Parse(userIdClaim.Value);

                // Find doctor profile
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Doctor profile not found",
                        Data = null
                    });
                }

                var appointmentList = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                        .ThenInclude(d => d.User)
                    .Include(a => a.Prescriptions)
                    .Where(a => a.DoctorId == doctor.Id)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.AppointmentTime)
                    .ToListAsync();

                var appointments = appointmentList.Select(a => new AppointmentResponseDto
                {
                    Id = a.Id.ToString(),
                    PatientId = a.PatientId.ToString(),
                    PatientName = a.Patient.FullName,
                    DoctorId = a.DoctorId.ToString(),
                    DoctorName = a.Doctor.User.FullName,
                    Specialization = a.Doctor.Specialization,
                    DateTime = new DateTime(a.AppointmentDate.Year, a.AppointmentDate.Month, a.AppointmentDate.Day,
                                          a.AppointmentTime.Hour, a.AppointmentTime.Minute, a.AppointmentTime.Second).ToString("o"),
                    Status = a.Status,
                    Symptoms = a.Symptoms,
                    Notes = a.Notes,
                    Prescription = a.Prescriptions.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.Notes,
                    CreatedAt = a.CreatedAt.HasValue ? a.CreatedAt.Value.ToString("o") : null
                }).ToList();

                return Ok(new ApiResponse<List<AppointmentResponseDto>>
                {
                    Success = true,
                    Message = "Appointments retrieved successfully",
                    Data = appointments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to retrieve appointments: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get appointment by ID
        /// </summary>
        [HttpGet("{appointmentId}")]
        public async Task<IActionResult> GetAppointment(string appointmentId)
        {
            try
            {
                if (!int.TryParse(appointmentId, out int id))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid appointment ID",
                        Data = null
                    });
                }

                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                        .ThenInclude(d => d.User)
                    .Include(a => a.Prescriptions)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (appointment == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Appointment not found",
                        Data = null
                    });
                }

                var responseData = new AppointmentResponseDto
                {
                    Id = appointment.Id.ToString(),
                    PatientId = appointment.PatientId.ToString(),
                    PatientName = appointment.Patient.FullName,
                    DoctorId = appointment.DoctorId.ToString(),
                    DoctorName = appointment.Doctor.User.FullName,
                    Specialization = appointment.Doctor.Specialization,
                    DateTime = new DateTime(appointment.AppointmentDate.Year, appointment.AppointmentDate.Month, appointment.AppointmentDate.Day,
                                          appointment.AppointmentTime.Hour, appointment.AppointmentTime.Minute, appointment.AppointmentTime.Second).ToString("o"),
                    Status = appointment.Status,
                    Symptoms = appointment.Symptoms,
                    Notes = appointment.Notes,
                    Prescription = appointment.Prescriptions.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.Notes,
                    CreatedAt = appointment.CreatedAt.HasValue ? appointment.CreatedAt.Value.ToString("o") : null
                };

                return Ok(new ApiResponse<AppointmentResponseDto>
                {
                    Success = true,
                    Message = "Appointment retrieved successfully",
                    Data = responseData
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to retrieve appointment: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Update appointment status
        /// </summary>
        [HttpPut("{appointmentId}/status")]
        [Authorize(Roles = "doctor,Faculty,admin")]
        public async Task<IActionResult> UpdateAppointmentStatus(string appointmentId, [FromBody] UpdateAppointmentStatusDto statusDto)
        {
            try
            {
                if (!int.TryParse(appointmentId, out int id))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid appointment ID",
                        Data = null
                    });
                }

                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Appointment not found",
                        Data = null
                    });
                }

                var validStatuses = new[] { "Pending", "Confirmed", "Completed", "Cancelled" };
                if (!validStatuses.Contains(statusDto.Status))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid status. Valid values: Pending, Confirmed, Completed, Cancelled",
                        Data = null
                    });
                }

                appointment.Status = statusDto.Status;
                appointment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Appointment status updated successfully",
                    Data = new { status = appointment.Status }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to update status: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Cancel appointment
        /// </summary>
        [HttpDelete("{appointmentId}")]
        public async Task<IActionResult> CancelAppointment(string appointmentId)
        {
            try
            {
                if (!int.TryParse(appointmentId, out int id))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid appointment ID",
                        Data = null
                    });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid token",
                        Data = null
                    });
                }

                var userId = int.Parse(userIdClaim.Value);

                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Appointment not found",
                        Data = null
                    });
                }

                if (appointment.Status == "Cancelled")
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Appointment is already cancelled",
                        Data = null
                    });
                }

                appointment.Status = "Cancelled";
                appointment.CancelledBy = userId;
                appointment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Appointment cancelled successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to cancel appointment: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Update appointment details
        /// </summary>
        [HttpPut("{appointmentId}")]
        public async Task<IActionResult> UpdateAppointment(string appointmentId, [FromBody] UpdateAppointmentDto updateDto)
        {
            try
            {
                if (!int.TryParse(appointmentId, out int id))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid appointment ID",
                        Data = null
                    });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid token",
                        Data = null
                    });
                }

                var userId = int.Parse(userIdClaim.Value);

                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Appointment not found",
                        Data = null
                    });
                }

                if (appointment.PatientId != userId && !User.IsInRole("admin"))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "You are not authorized to update this appointment",
                        Data = null
                    });
                }

                // Allow updates only if status is Pending
                if (appointment.Status != "Pending")
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Cannot update appointment that is no longer Pending",
                        Data = null
                    });
                }

                // Parse doctor ID
                if (!int.TryParse(updateDto.DoctorId, out int doctorId))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid doctor ID",
                        Data = null
                    });
                }

                // Check for conflict if date/time or doctor changed
                var newDateTime = DateTime.Parse(updateDto.DateTime);
                var newDate = DateOnly.FromDateTime(newDateTime);
                var newTime = TimeOnly.FromDateTime(newDateTime);

                if (appointment.DoctorId != doctorId || appointment.AppointmentDate != newDate || appointment.AppointmentTime != newTime)
                {
                    var conflictingAppointment = await _context.Appointments
                   .Where(a => a.DoctorId == doctorId
                       && a.AppointmentDate == newDate
                       && a.AppointmentTime == newTime
                       && a.Status != "Cancelled"
                       && a.Id != id)
                   .FirstOrDefaultAsync();

                    if (conflictingAppointment != null)
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = "This time slot is already booked",
                            Data = null
                        });
                    }
                }

                appointment.DoctorId = doctorId;
                appointment.AppointmentDate = newDate;
                appointment.AppointmentTime = newTime;
                appointment.Symptoms = updateDto.Symptoms;
                appointment.Notes = updateDto.Notes;
                appointment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Appointment updated successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to update appointment: {ex.Message}",
                    Data = null
                });
            }
        }

        private async Task<DoctorBookingSettingsDto> GetDoctorBookingSettingsAsync(int doctorId)
        {
            var defaults = new DoctorBookingSettingsDto();
            var settingKey = $"{BookingSettingsPrefix}{doctorId}";

            var setting = await _context.Systemsettings.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SettingKey == settingKey);

            if (setting == null || string.IsNullOrWhiteSpace(setting.SettingValue))
            {
                return defaults;
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<DoctorBookingSettingsDto>(setting.SettingValue);
                if (parsed == null)
                {
                    return defaults;
                }

                if (!AllowedSlotDurations.Contains(parsed.AppointmentDuration))
                {
                    parsed.AppointmentDuration = 30;
                }

                parsed.MaxPatientsPerDay = Math.Clamp(parsed.MaxPatientsPerDay, 1, 50);
                parsed.ReminderNotificationMinutes = Math.Clamp(parsed.ReminderNotificationMinutes, 5, 120);

                if (!TimeOnly.TryParse(parsed.BreakStartTime, out _))
                {
                    parsed.BreakStartTime = "12:00";
                }

                if (!TimeOnly.TryParse(parsed.BreakEndTime, out _))
                {
                    parsed.BreakEndTime = "13:00";
                }

                return parsed;
            }
            catch
            {
                return defaults;
            }
        }

        /// <summary>
        /// Add prescription to appointment
        /// </summary>
        [HttpPut("{appointmentId}/prescription")]
        [Authorize(Roles = "doctor,Faculty,admin")]
        public async Task<IActionResult> AddPrescription(string appointmentId, [FromBody] AddPrescriptionDto prescriptionDto)
        {
            try
            {
                if (!int.TryParse(appointmentId, out int id))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid appointment ID",
                        Data = null
                    });
                }

                var appointment = await _context.Appointments
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (appointment == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Appointment not found",
                        Data = null
                    });
                }

                // Create prescription
                var prescription = new Prescription
                {
                    AppointmentId = id,
                    Diagnosis = "Consultation completed",
                    Notes = prescriptionDto.Prescription,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Prescriptions.Add(prescription);

                // Update appointment status to Completed
                appointment.Status = "Completed";
                appointment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Prescription added successfully",
                    Data = new { prescriptionId = prescription.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to add prescription: {ex.Message}",
                    Data = null
                });
            }
        }
    }

    // DTOs for Appointments
    public class CreateAppointmentDto
    {
        public string PatientId { get; set; } = null!;
        public string? PatientName { get; set; }
        public string DoctorId { get; set; } = null!;
        public string? DoctorName { get; set; }
        public string? Specialization { get; set; }
        public string DateTime { get; set; } = null!; // ISO 8601 format
        public string? Symptoms { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "Pending";
    }

    public class AppointmentResponseDto
    {
        public string Id { get; set; } = null!;
        public string PatientId { get; set; } = null!;
        public string PatientName { get; set; } = null!;
        public string DoctorId { get; set; } = null!;
        public string DoctorName { get; set; } = null!;
        public string Specialization { get; set; } = null!;
        public string DateTime { get; set; } = null!;
        public string? Status { get; set; }
        public string? Symptoms { get; set; }
        public string? Notes { get; set; }
        public string? Prescription { get; set; }
        public string? CreatedAt { get; set; }
    }

    public class UpdateAppointmentStatusDto
    {
        public string Status { get; set; } = null!;
    }

    public class AddPrescriptionDto
    {
        public string Prescription { get; set; } = null!;
    }

    public class UpdateAppointmentDto
    {
        public string DoctorId { get; set; } = null!;
        public string DateTime { get; set; } = null!; // ISO 8601 format
        public string? Symptoms { get; set; }
        public string? Notes { get; set; }
    }
}
