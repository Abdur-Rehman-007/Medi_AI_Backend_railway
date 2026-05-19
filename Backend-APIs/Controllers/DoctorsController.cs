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
    public class DoctorsController : ControllerBase
    {
        private readonly MediaidbContext _context;
        private const string BookingSettingsPrefix = "DoctorBookingSettings:";
        private static readonly HashSet<int> AllowedSlotDurations = new() { 15, 20, 30, 45, 60 };
        private static readonly HashSet<string> AllowedDays = new(StringComparer.OrdinalIgnoreCase)
        {
            "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
        };
        private static readonly TimeOnly UniversityStartTime = new(8, 0);
        private static readonly TimeOnly UniversityEndTime = new(17, 0);

        public DoctorsController(MediaidbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 🚀 THE MAGIC FIX: Auto-Create Doctor Profile
        // ==========================================
        private async Task<Doctor> EnsureDoctorProfileExists(int userId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Doctorschedules)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null && (user.Role.ToLower() == "doctor"))
                {
                    doctor = new Doctor
                    {
                        UserId = user.Id,
                        Specialization = "General Physician",
                        LicenseNumber = $"TEMP-{user.Id}-{DateTime.UtcNow.Ticks}",
                        Qualification = "MBBS",
                        Experience = 5,
                        IsAvailable = true,
                        Bio = "Experienced doctor available for consultation.",
                        AverageRating = 0,
                        TotalRatings = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Doctors.Add(doctor);
                    await _context.SaveChangesAsync();

                    // Reload with includes so subsequent code doesn't crash
                    doctor = await _context.Doctors
                        .Include(d => d.User)
                        .Include(d => d.Doctorschedules)
                        .FirstOrDefaultAsync(d => d.UserId == userId);
                }
            }
            return doctor;
        }

        /// <summary>
        /// Get current doctor profile
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                var userId = int.Parse(userIdClaim.Value);

                var doctor = await EnsureDoctorProfileExists(userId);
                if (doctor == null) return NotFound("Doctor profile not found");

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Profile loaded",
                    Data = new
                    {
                        doctor.Id,
                        doctor.UserId,
                        doctor.Specialization,
                        doctor.Qualification,
                        doctor.Experience,
                        doctor.RoomNumber,
                        doctor.Bio,
                        doctor.IsAvailable,
                        User = new
                        {
                            doctor.User.FullName,
                            doctor.User.Email,
                            doctor.User.PhoneNumber,
                            doctor.User.ProfileImageUrl
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Update current doctor profile
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateDoctorProfile([FromBody] UpdateDoctorProfileDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                var userId = int.Parse(userIdClaim.Value);

                var doctor = await EnsureDoctorProfileExists(userId);
                if (doctor == null) return NotFound(new ApiResponse<object> { Success = false, Message = "Doctor profile not found" });

                if (!string.IsNullOrEmpty(dto.FullName)) doctor.User.FullName = dto.FullName;
                if (!string.IsNullOrEmpty(dto.PhoneNumber)) doctor.User.PhoneNumber = dto.PhoneNumber;
                if (!string.IsNullOrEmpty(dto.Specialization)) doctor.Specialization = dto.Specialization;
                if (!string.IsNullOrEmpty(dto.RoomNumber)) doctor.RoomNumber = dto.RoomNumber;
                if (!string.IsNullOrEmpty(dto.Bio)) doctor.Bio = dto.Bio;
                if (dto.IsAvailable.HasValue) doctor.IsAvailable = dto.IsAvailable;

                _context.Update(doctor);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Profile updated successfully",
                    Data = new
                    {
                        doctor.Id,
                        doctor.Specialization,
                        doctor.RoomNumber,
                        doctor.Bio,
                        doctor.IsAvailable,
                        User = new
                        {
                            doctor.User.FullName,
                            doctor.User.Email,
                            doctor.User.PhoneNumber
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Get all doctors with user details
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetDoctors()
        {
            var doctors = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Select(d => new
                {
                    d.Id,
                    d.UserId,
                    d.Specialization,
                    d.LicenseNumber,
                    d.Qualification,
                    d.Experience,
                    d.RoomNumber,
                    d.Bio,
                    d.AverageRating,
                    d.TotalRatings,
                    d.IsAvailable,
                    d.CreatedAt,
                    User = new
                    {
                        d.User.Id,
                        d.User.FullName,
                        d.User.Email,
                        d.User.PhoneNumber,
                        d.User.ProfileImageUrl
                    }
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Doctors loaded successfully",
                Data = doctors
            });
        }

        /// <summary>
        /// Get doctor by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetDoctor(int id)
        {
            var doctor = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.Doctorschedules)
                .Include(d => d.Doctorreviews)
                    .ThenInclude(r => r.Patient)
                .Where(d => d.Id == id)
                .Select(d => new
                {
                    d.Id,
                    d.UserId,
                    d.Specialization,
                    d.LicenseNumber,
                    d.Qualification,
                    d.Experience,
                    d.RoomNumber,
                    d.Bio,
                    d.AverageRating,
                    d.TotalRatings,
                    d.IsAvailable,
                    d.CreatedAt,
                    User = new
                    {
                        d.User.Id,
                        d.User.FullName,
                        d.User.Email,
                        d.User.PhoneNumber,
                        d.User.ProfileImageUrl,
                        d.User.Gender,
                        d.User.Department
                    },
                    Schedules = d.Doctorschedules.Select(s => new
                    {
                        s.Id,
                        s.DayOfWeek,
                        s.StartTime,
                        s.EndTime,
                        s.IsActive
                    }),
                    Reviews = d.Doctorreviews.Select(r => new
                    {
                        r.Id,
                        r.Rating,
                        r.Review,
                        r.CreatedAt,
                        PatientName = r.Patient.FullName
                    })
                })
                .FirstOrDefaultAsync();

            if (doctor == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Doctor not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Doctor details loaded",
                Data = doctor
            });
        }

        /// <summary>
        /// Get doctors by specialization
        /// </summary>
        [HttpGet("specialization/{specialization}")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetDoctorsBySpecialization(string specialization)
        {
            var doctors = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Where(d => d.Specialization.ToLower().Contains(specialization.ToLower()))
                .Select(d => new
                {
                    d.Id,
                    d.UserId,
                    d.Specialization,
                    d.Qualification,
                    d.Experience,
                    d.AverageRating,
                    d.TotalRatings,
                    d.IsAvailable,
                    User = new
                    {
                        d.User.FullName,
                        d.User.ProfileImageUrl
                    }
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Doctors loaded successfully",
                Data = doctors
            });
        }

        /// <summary>
        /// Update doctor availability
        /// </summary>
        [HttpPatch("{id}/availability")]
        [Authorize(Roles = "doctor,admin")]
        public async Task<IActionResult> UpdateAvailability(int id, [FromBody] bool isAvailable)
        {
            var doctor = await _context.Doctors.FindAsync(id);

            if (doctor == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Doctor not found",
                    Data = null,
                    Errors = null
                });
            }

            doctor.IsAvailable = isAvailable;
            doctor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Availability updated successfully",
                Data = new { isAvailable }
            });
        }

        /// <summary>
        /// Update doctor profile
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "doctor,admin")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] Doctor doctor)
        {
            if (id != doctor.Id)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ID mismatch",
                    Data = null,
                    Errors = null
                });
            }

            var existingDoctor = await _context.Doctors.FindAsync(id);
            if (existingDoctor == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Doctor not found",
                    Data = null,
                    Errors = null
                });
            }

            existingDoctor.Specialization = doctor.Specialization;
            existingDoctor.LicenseNumber = doctor.LicenseNumber;
            existingDoctor.Qualification = doctor.Qualification;
            existingDoctor.Experience = doctor.Experience;
            existingDoctor.RoomNumber = doctor.RoomNumber;
            existingDoctor.Bio = doctor.Bio;
            existingDoctor.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error updating doctor"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Doctor updated successfully"
            });
        }

        /// <summary>
        /// Get available doctors for appointment booking
        /// </summary>
        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetAvailableDoctors()
        {
            var doctorUsers = await _context.Users
                .Where(u => (u.Role == "Doctor" || u.Role == "doctor") && !_context.Doctors.Any(d => d.UserId == u.Id))
                .ToListAsync();

            if (doctorUsers.Any())
            {
                foreach (var user in doctorUsers)
                {
                    var newDoctor = new Doctor
                    {
                        UserId = user.Id,
                        Specialization = "General Physician",
                        LicenseNumber = $"TEMP-{user.Id}-{DateTime.UtcNow.Ticks}",
                        Qualification = "MBBS",
                        Experience = 5,
                        IsAvailable = true,
                        Bio = "Experienced doctor available for consultation.",
                        AverageRating = 0,
                        TotalRatings = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Doctors.Add(newDoctor);
                }
                await _context.SaveChangesAsync();
            }

            var doctors = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Where(d => d.IsAvailable == true && d.User.IsActive == true)
                .Select(d => new
                {
                    d.Id,
                    d.Specialization,
                    d.AverageRating,
                    d.Experience,
                    User = new
                    {
                        d.User.FullName,
                        d.User.ProfileImageUrl
                    }
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Available doctors loaded successfully",
                Data = doctors
            });
        }

        /// <summary>
        /// Search doctors by name, specialization, or other criteria
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchDoctors(
            [FromQuery] string? query,
            [FromQuery] string? specialization,
            [FromQuery] bool? availableOnly = false)
        {
            try
            {
                var doctorsQuery = _context.Doctors
                    .AsNoTracking()
                    .Where(d => d.User.IsActive == true)
                    .AsQueryable();

                if (availableOnly == true)
                {
                    doctorsQuery = doctorsQuery.Where(d => d.IsAvailable == true);
                }

                if (!string.IsNullOrWhiteSpace(specialization))
                {
                    doctorsQuery = doctorsQuery.Where(d =>
                        d.Specialization.ToLower().Contains(specialization.ToLower()));
                }

                if (!string.IsNullOrWhiteSpace(query))
                {
                    doctorsQuery = doctorsQuery.Where(d =>
                        d.User.FullName.ToLower().Contains(query.ToLower()) ||
                        d.Specialization.ToLower().Contains(query.ToLower()));
                }

                var doctors = await doctorsQuery
                    .Select(d => new DoctorSearchDto
                    {
                        DoctorId = d.Id,
                        DoctorName = d.User.FullName,
                        Specialization = d.Specialization,
                        Qualifications = d.Qualification,
                        ExperienceYears = d.Experience,
                        IsAvailable = d.IsAvailable ?? false,
                        AverageRating = d.AverageRating.HasValue ? (double)d.AverageRating.Value : null,
                        TotalReviews = d.TotalRatings,
                        ProfileImageUrl = d.User.ProfileImageUrl,
                        Bio = d.Bio
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<DoctorSearchDto>>
                {
                    Success = true,
                    Message = $"Found {doctors.Count} doctor(s)",
                    Data = doctors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Search failed: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get list of all specializations with doctor count
        /// </summary>
        [HttpGet("specializations")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSpecializations()
        {
            try
            {
                var specializations = await _context.Doctors
                    .Where(d => d.User.IsActive == true && !string.IsNullOrEmpty(d.Specialization))
                    .GroupBy(d => d.Specialization)
                    .Select(g => new SpecializationDto
                    {
                        Specialization = g.Key,
                        DoctorCount = g.Count()
                    })
                    .OrderBy(s => s.Specialization)
                    .ToListAsync();

                return Ok(new ApiResponse<List<SpecializationDto>>
                {
                    Success = true,
                    Message = "Specializations retrieved successfully",
                    Data = specializations
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to retrieve specializations: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get available time slots for a doctor on a specific date
        /// </summary>
        [HttpGet("{id}/available-slots")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableSlots(int id, [FromQuery] string date)
        {
            try
            {
                if (!DateOnly.TryParse(date, out var requestedDate))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid date format. Use yyyy-MM-dd",
                        Data = null
                    });
                }

                if (requestedDate < DateOnly.FromDateTime(DateTime.Today))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Cannot get slots for past dates",
                        Data = null
                    });
                }

                var doctor = await _context.Doctors
                    .AsNoTracking()
                    .Include(d => d.User)
                    .Include(d => d.Doctorschedules)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (doctor == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Doctor not found",
                        Data = null
                    });
                }

                var dayOfWeek = requestedDate.DayOfWeek.ToString();

                var schedule = doctor.Doctorschedules
                    .FirstOrDefault(s => s.DayOfWeek == dayOfWeek && s.IsActive == true);

                if (schedule == null)
                {
                    return Ok(new ApiResponse<AvailableSlotsResponseDto>
                    {
                        Success = true,
                        Message = "Doctor is not available on this day",
                        Data = new AvailableSlotsResponseDto
                        {
                            Date = date,
                            DoctorId = id,
                            DoctorName = doctor.User.FullName,
                            Slots = new List<AvailableSlotDto>()
                        }
                    });
                }

                var existingAppointments = await _context.Appointments
                    .AsNoTracking()
                    .Where(a => a.DoctorId == id
                        && a.AppointmentDate == requestedDate
                        && a.Status != "Cancelled")
                    .Select(a => a.AppointmentTime)
                    .ToListAsync();

                var slots = new List<AvailableSlotDto>();
                var startTime = schedule.StartTime;
                var endTime = schedule.EndTime;
                var bookingSettings = await GetDoctorBookingSettingsInternalAsync(id);
                var slotDuration = bookingSettings.AppointmentDuration;
                TimeOnly.TryParse(bookingSettings.BreakStartTime, out var breakStartTime);
                TimeOnly.TryParse(bookingSettings.BreakEndTime, out var breakEndTime);

                var currentTime = startTime;
                while (currentTime < endTime)
                {
                    var isInBreak = bookingSettings.EnableBreakTime
                                    && breakStartTime < breakEndTime
                                    && currentTime >= breakStartTime
                                    && currentTime < breakEndTime;

                    if (isInBreak)
                    {
                        currentTime = currentTime.AddMinutes(slotDuration);
                        continue;
                    }

                    var isBooked = existingAppointments.Any(apt => apt == currentTime);

                    slots.Add(new AvailableSlotDto
                    {
                        Time = currentTime.ToString("HH:mm"),
                        Duration = slotDuration,
                        Available = !isBooked
                    });

                    currentTime = currentTime.AddMinutes(slotDuration);
                }

                var response = new AvailableSlotsResponseDto
                {
                    Date = date,
                    DoctorId = id,
                    DoctorName = doctor.User.FullName,
                    Slots = slots
                };

                return Ok(new ApiResponse<AvailableSlotsResponseDto>
                {
                    Success = true,
                    Message = "Available slots retrieved successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to retrieve available slots: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get doctor's schedule for the week
        /// </summary>
        [HttpGet("{id}/schedule")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDoctorSchedule(int id)
        {
            try
            {
                var doctor = await _context.Doctors
                    .AsNoTracking()
                    .Include(d => d.User)
                    .Include(d => d.Doctorschedules)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (doctor == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Doctor not found",
                        Data = null
                    });
                }

                var schedules = doctor.Doctorschedules
                    .Where(s => s.IsActive == true)
                    .Select(s => new DoctorScheduleDto
                    {
                        ScheduleId = s.Id,
                        DayOfWeek = s.DayOfWeek,
                        StartTime = s.StartTime.ToString("HH:mm"),
                        EndTime = s.EndTime.ToString("HH:mm"),
                        IsAvailable = s.IsActive ?? false
                    })
                    .OrderBy(s => GetDayOrder(s.DayOfWeek))
                    .ToList();

                return Ok(new ApiResponse<List<DoctorScheduleDto>>
                {
                    Success = true,
                    Message = "Schedule retrieved successfully",
                    Data = schedules
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to retrieve schedule: {ex.Message}",
                    Data = null
                });
            }
        }

        private int GetDayOrder(string dayOfWeek)
        {
            return dayOfWeek switch
            {
                "Monday" => 1,
                "Tuesday" => 2,
                "Wednesday" => 3,
                "Thursday" => 4,
                "Friday" => 5,
                "Saturday" => 6,
                "Sunday" => 7,
                _ => 8
            };
        }

        [HttpGet("my-booking-settings")]
        [Authorize(Roles = "doctor,Doctor")]
        public async Task<IActionResult> GetMyBookingSettings()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                var userId = int.Parse(userIdClaim.Value);

                var doctor = await EnsureDoctorProfileExists(userId);
                if (doctor == null) return NotFound("Doctor profile not found");

                var settings = await GetDoctorBookingSettingsInternalAsync(doctor.Id);

                return Ok(new ApiResponse<DoctorBookingSettingsDto>
                {
                    Success = true,
                    Message = "Booking settings loaded",
                    Data = settings
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to load booking settings: {ex.Message}",
                    Data = null
                });
            }
        }

        [HttpPut("my-booking-settings")]
        [Authorize(Roles = "doctor,Doctor")]
        public async Task<IActionResult> UpdateMyBookingSettings([FromBody] DoctorBookingSettingsDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                var userId = int.Parse(userIdClaim.Value);

                var doctor = await EnsureDoctorProfileExists(userId);
                if (doctor == null) return NotFound("Doctor profile not found");

                NormalizeBookingSettings(dto);

                var settingKey = $"{BookingSettingsPrefix}{doctor.Id}";
                var existing = await _context.Systemsettings.FirstOrDefaultAsync(s => s.SettingKey == settingKey);

                if (existing == null)
                {
                    existing = new Systemsetting
                    {
                        SettingKey = settingKey,
                        Description = "Per-doctor booking and reminder settings",
                        DataType = "JSON",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.Systemsettings.AddAsync(existing);
                }

                existing.SettingValue = JsonSerializer.Serialize(dto);
                existing.UpdatedBy = userId;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<DoctorBookingSettingsDto>
                {
                    Success = true,
                    Message = "Booking settings saved",
                    Data = dto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed to save booking settings: {ex.Message}",
                    Data = null
                });
            }
        }

        private async Task<DoctorBookingSettingsDto> GetDoctorBookingSettingsInternalAsync(int doctorId)
        {
            var defaults = new DoctorBookingSettingsDto();
            var settingKey = $"{BookingSettingsPrefix}{doctorId}";
            var entity = await _context.Systemsettings.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SettingKey == settingKey);

            if (entity == null || string.IsNullOrWhiteSpace(entity.SettingValue))
            {
                return defaults;
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<DoctorBookingSettingsDto>(entity.SettingValue);
                if (parsed == null)
                {
                    return defaults;
                }

                NormalizeBookingSettings(parsed);
                return parsed;
            }
            catch
            {
                return defaults;
            }
        }

        private static void NormalizeBookingSettings(DoctorBookingSettingsDto settings)
        {
            if (!AllowedSlotDurations.Contains(settings.AppointmentDuration))
            {
                settings.AppointmentDuration = 30;
            }

            settings.MaxPatientsPerDay = Math.Clamp(settings.MaxPatientsPerDay, 1, 50);
            settings.ReminderNotificationMinutes = Math.Clamp(settings.ReminderNotificationMinutes, 5, 120);

            if (!TimeOnly.TryParse(settings.BreakStartTime, out _))
            {
                settings.BreakStartTime = "12:00";
            }

            if (!TimeOnly.TryParse(settings.BreakEndTime, out _))
            {
                settings.BreakEndTime = "13:00";
            }
        }

        [HttpGet("appointments/today")]
        public async Task<IActionResult> GetTodayAppointments()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                var userId = int.Parse(userIdClaim.Value);

                var doctor = await EnsureDoctorProfileExists(userId);
                if (doctor == null) return NotFound("Doctor profile not found");

                var today = DateOnly.FromDateTime(DateTime.Today);
                var appointments = await _context.Appointments
                    .AsNoTracking()
                    .Include(a => a.Patient)
                    .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate == today)
                    .Select(a => new AppointmentResponseDto
                    {
                        Id = a.Id.ToString(),
                        PatientId = a.PatientId.ToString(),
                        PatientName = a.Patient.FullName,
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
                    Message = "Today's appointments loaded",
                    Data = appointments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Get all appointments for the logged-in doctor
        /// </summary>
        [HttpGet("appointments")]
        [Authorize(Roles = "doctor,Doctor")]
        public async Task<IActionResult> GetAllDoctorAppointments()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                var userId = int.Parse(userIdClaim.Value);

                var doctor = await EnsureDoctorProfileExists(userId);
                if (doctor == null) return NotFound("Doctor profile not found");

                var appointments = await _context.Appointments
                    .AsNoTracking()
                    .Include(a => a.Patient)
                    .Where(a => a.DoctorId == doctor.Id)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.AppointmentTime)
                    .Select(a => new AppointmentResponseDto
                    {
                        Id = a.Id.ToString(),
                        PatientId = a.PatientId.ToString(),
                        PatientName = a.Patient.FullName,
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
                    Message = "All appointments retrieved",
                    Data = appointments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Get all unique patients for the logged-in doctor
        /// </summary>
        [HttpGet("patients")]
        [Authorize(Roles = "doctor,Doctor")]
        public async Task<IActionResult> GetDoctorPatients()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                var userId = int.Parse(userIdClaim.Value);

                var doctor = await EnsureDoctorProfileExists(userId);
                if (doctor == null) return NotFound("Doctor profile not found");

                var patientIds = await _context.Appointments
                    .AsNoTracking()
                    .Where(a => a.DoctorId == doctor.Id)
                    .Select(a => a.PatientId)
                    .Distinct()
                    .ToListAsync();

                var patients = await _context.Users
                    .AsNoTracking()
                    .Where(u => patientIds.Contains(u.Id))
                    .Select(u => new
                    {
                        u.Id,
                        u.FullName,
                        u.Email,
                        u.PhoneNumber,
                        u.RegistrationNumber,
                        u.Gender,
                        u.DateOfBirth,
                        u.ProfileImageUrl,
                        LastVisit = _context.Appointments
                            .Where(a => a.PatientId == u.Id && a.DoctorId == doctor.Id)
                            .OrderByDescending(a => a.AppointmentDate)
                            .Select(a => a.AppointmentDate)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Patients retrieved successfully",
                    Data = patients
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Get schedule for the logged-in doctor
        /// </summary>
        [HttpGet("my-schedule")]
        [Authorize(Roles = "doctor,Doctor")]
        public async Task<IActionResult> GetMySchedule()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                var userId = int.Parse(userIdClaim.Value);

                var doctor = await EnsureDoctorProfileExists(userId);
                if (doctor == null) return NotFound("Doctor profile not found");

                List<DoctorScheduleDto> schedules;
                if (!doctor.Doctorschedules.Any())
                {
                    schedules = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" }
                        .Select(day => new DoctorScheduleDto
                        {
                            DayOfWeek = day,
                            StartTime = "09:00",
                            EndTime = "17:00",
                            IsAvailable = false
                        })
                        .ToList();
                }
                else
                {
                    schedules = doctor.Doctorschedules
                        .Select(s => new DoctorScheduleDto
                        {
                            ScheduleId = s.Id,
                            DayOfWeek = s.DayOfWeek,
                            StartTime = s.StartTime.ToString("HH:mm"),
                            EndTime = s.EndTime.ToString("HH:mm"),
                            IsAvailable = s.IsActive ?? false
                        })
                        .OrderBy(s => GetDayOrder(s.DayOfWeek))
                        .ToList();
                }

                return Ok(new ApiResponse<List<DoctorScheduleDto>>
                {
                    Success = true,
                    Message = "Schedule retrieved successfully",
                    Data = schedules
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Update schedule for the logged-in doctor
        /// </summary>
        [HttpPost("schedule")]
        [Authorize(Roles = "doctor,Doctor")]
        public async Task<IActionResult> UpdateSchedule([FromBody] UpdateScheduleDto dto)
        {
            try
            {
                if (dto == null || dto.Schedules == null || dto.Schedules.Count == 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "At least one schedule day is required"
                    });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                var userId = int.Parse(userIdClaim.Value);

                var doctor = await EnsureDoctorProfileExists(userId);
                if (doctor == null) return NotFound("Doctor profile not found");

                var normalizedDays = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var s in dto.Schedules)
                {
                    if (string.IsNullOrWhiteSpace(s.DayOfWeek) || !AllowedDays.Contains(s.DayOfWeek))
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = $"Invalid day: {s.DayOfWeek}"
                        });
                    }

                    if (!normalizedDays.Add(s.DayOfWeek.Trim()))
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = $"Duplicate day found: {s.DayOfWeek}"
                        });
                    }

                    if (!TimeOnly.TryParse(s.StartTime, out var startTime) || !TimeOnly.TryParse(s.EndTime, out var endTime))
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = $"Invalid time format for {s.DayOfWeek}. Use HH:mm"
                        });
                    }

                    if (startTime >= endTime)
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = $"Start time must be before end time for {s.DayOfWeek}"
                        });
                    }

                    if (startTime < UniversityStartTime || endTime > UniversityEndTime)
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = $"Schedule for {s.DayOfWeek} must be within university timings (08:00-17:00)"
                        });
                    }
                }

                _context.Doctorschedules.RemoveRange(doctor.Doctorschedules);

                var newSchedules = dto.Schedules.Select(s => new Doctorschedule
                {
                    DoctorId = doctor.Id,
                    DayOfWeek = s.DayOfWeek,
                    StartTime = TimeOnly.Parse(s.StartTime),
                    EndTime = TimeOnly.Parse(s.EndTime),
                    IsActive = s.IsAvailable,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _context.Doctorschedules.AddRangeAsync(newSchedules);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Schedule updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("appointments/upcoming")]
        public async Task<IActionResult> GetUpcomingAppointments()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                var userId = int.Parse(userIdClaim.Value);

                var doctor = await EnsureDoctorProfileExists(userId);
                if (doctor == null) return NotFound("Doctor profile not found");

                var today = DateOnly.FromDateTime(DateTime.Today);
                var appointments = await _context.Appointments
                    .AsNoTracking()
                    .Include(a => a.Patient)
                    .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate > today)
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentTime)
                    .Take(10)
                    .Select(a => new AppointmentResponseDto
                    {
                        Id = a.Id.ToString(),
                        PatientId = a.PatientId.ToString(),
                        PatientName = a.Patient.FullName,
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
                    Message = "Upcoming appointments loaded",
                    Data = appointments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Get doctor dashboard statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetDashboardStatistics()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                var userId = int.Parse(userIdClaim.Value);

                var doctor = await EnsureDoctorProfileExists(userId);
                if (doctor == null) return NotFound("Doctor profile not found");

                var today = DateOnly.FromDateTime(DateTime.Today);
                var now = TimeOnly.FromDateTime(DateTime.Now);

                var todayTotal = await _context.Appointments
                    .AsNoTracking()
                    .CountAsync(a => a.DoctorId == doctor.Id && a.AppointmentDate == today);

                var pendingToday = await _context.Appointments
                    .AsNoTracking()
                    .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate == today && a.Status == "Pending")
                    .CountAsync();

                var completedToday = await _context.Appointments
                    .AsNoTracking()
                    .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate == today && (a.Status == "Completed" || a.Status == "Checked"))
                    .CountAsync();

                var totalPatients = await _context.Appointments
                    .AsNoTracking()
                    .Where(a => a.DoctorId == doctor.Id)
                    .Select(a => a.PatientId)
                    .Distinct()
                    .CountAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Statistics loaded",
                    Data = new
                    {
                        totalPatients,
                        todayTotal,
                        completedToday,
                        pendingToday
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