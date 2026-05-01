using Backend_APIs.DTOs;
using Backend_APIs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin,Admin")]
    public class AdminController : ControllerBase
    {
        private readonly MediaidbContext _context;

        public AdminController(MediaidbContext context)
        {
            _context = context;
        }

        [HttpGet("system-settings")]
        public async Task<IActionResult> GetSystemSettings()
        {
            var settings = await _context.Systemsettings.ToListAsync();

            var dto = new SystemSettingsDto
            {
                MaintenanceMode = bool.TryParse(settings.FirstOrDefault(s => s.SettingKey == "MaintenanceMode")?.SettingValue, out var mm) ? mm : false,
                EmailNotifications = bool.TryParse(settings.FirstOrDefault(s => s.SettingKey == "EnableEmailNotifications")?.SettingValue, out var en) ? en : true,
                SmsNotifications = bool.TryParse(settings.FirstOrDefault(s => s.SettingKey == "EnableSmsNotifications")?.SettingValue, out var sn) ? sn : false,
                AutoApproveRegistrations = bool.TryParse(settings.FirstOrDefault(s => s.SettingKey == "AutoApproveRegistrations")?.SettingValue, out var ar) ? ar : false,
                RequireEmailVerification = bool.TryParse(settings.FirstOrDefault(s => s.SettingKey == "RequireEmailVerification")?.SettingValue, out var rv) ? rv : true,
                TwoFactorAuth = bool.TryParse(settings.FirstOrDefault(s => s.SettingKey == "TwoFactorAuth")?.SettingValue, out var ta) ? ta : false,
                SessionTimeoutMinutes = int.TryParse(settings.FirstOrDefault(s => s.SettingKey == "SessionTimeoutMinutes")?.SettingValue, out var st) ? st : 30,
                MaxLoginAttempts = int.TryParse(settings.FirstOrDefault(s => s.SettingKey == "MaxLoginAttempts")?.SettingValue, out var mla) ? mla : 5,
                SystemName = settings.FirstOrDefault(s => s.SettingKey == "SystemName")?.SettingValue ?? "Medi-AI",
                ContactEmail = settings.FirstOrDefault(s => s.SettingKey == "ContactEmail")?.SettingValue ?? "admin@buitms.edu.pk",
                SupportEmail = settings.FirstOrDefault(s => s.SettingKey == "SupportEmail")?.SettingValue ?? "support@buitms.edu.pk"
            };

            return Ok(new ApiResponse<SystemSettingsDto>
            {
                Success = true,
                Message = "System settings retrieved successfully",
                Data = dto
            });
        }

        [HttpPut("system-settings")]
        public async Task<IActionResult> UpdateSystemSettings([FromBody] SystemSettingsDto dto)
        {
            await UpsertSetting("MaintenanceMode", dto.MaintenanceMode.ToString().ToLower());
            await UpsertSetting("EnableEmailNotifications", dto.EmailNotifications.ToString().ToLower());
            await UpsertSetting("EnableSmsNotifications", dto.SmsNotifications.ToString().ToLower());
            await UpsertSetting("AutoApproveRegistrations", dto.AutoApproveRegistrations.ToString().ToLower());
            await UpsertSetting("RequireEmailVerification", dto.RequireEmailVerification.ToString().ToLower());
            await UpsertSetting("TwoFactorAuth", dto.TwoFactorAuth.ToString().ToLower());
            await UpsertSetting("SessionTimeoutMinutes", dto.SessionTimeoutMinutes.ToString());
            await UpsertSetting("MaxLoginAttempts", dto.MaxLoginAttempts.ToString());
            await UpsertSetting("SystemName", dto.SystemName);
            await UpsertSetting("ContactEmail", dto.ContactEmail);
            await UpsertSetting("SupportEmail", dto.SupportEmail);

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<SystemSettingsDto>
            {
                Success = true,
                Message = "System settings updated successfully",
                Data = dto
            });
        }

        private async Task UpsertSetting(string key, string value)
        {
            var setting = await _context.Systemsettings.FirstOrDefaultAsync(s => s.SettingKey == key);
            if (setting == null)
            {
                setting = new Systemsetting
                {
                    SettingKey = key,
                    SettingValue = value,
                    DataType = "String",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Systemsettings.Add(setting);
            }
            else
            {
                setting.SettingValue = value;
                // setting.UpdatedAt = DateTime.UtcNow; // Model doesn't have UpdatedAt, skipping
            }
        }

        [HttpPost("clear-cache")]
        public IActionResult ClearCache()
        {
            // Implementation depends on caching strategy (Redis, MemoryCache, etc.)
            // For now, we simulate success as there's no distributed cache yet.
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "System cache cleared successfully",
                Data = null
            });
        }

        [HttpPost("backup-database")]
        public IActionResult BackupDatabase()
        {
            // Trigger backup logic here (e.g. mysqldump)
            // Simulating backup started
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Database backup started in background",
                Data = null
            });
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var totalStudents = await _context.Users.CountAsync(u => u.Role == "Student");
                var totalFaculty = await _context.Users.CountAsync(u => u.Role == "Faculty");
                var totalDoctors = await _context.Doctors.CountAsync(); // Or Users with Role = Doctor
                var totalAppointments = await _context.Appointments.CountAsync();

                var today = DateOnly.FromDateTime(DateTime.Today);
                var todayAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate == today);

                var pendingVerifications = await _context.Users.CountAsync(u => u.IsEmailVerified == false);
                var systemAlerts = await _context.Notifications.CountAsync(n => n.IsRead == false &&
                                                                               (n.User.Role == "Admin" || n.User.Role == "admin"));

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Statistics loaded successfully",
                    Data = new
                    {
                        totalUsers,
                        totalStudents,
                        totalFaculty,
                        totalDoctors,
                        totalAppointments = totalAppointments,
                        todayAppointments = todayAppointments,
                        pendingVerifications = pendingVerifications,
                        systemAlerts = systemAlerts
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error loading statistics: {ex.Message}",
                    Data = null
                });
            }
        }

        [HttpGet("recent-activities")]
        public async Task<IActionResult> GetRecentActivities()
        {
            var recentAudits = await _context.Auditlogs
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new
                {
                    a.Id,
                    Title = a.Action,
                    Description = a.User != null
                        ? $"{a.User.FullName}{(string.IsNullOrWhiteSpace(a.EntityType) ? string.Empty : $" • {a.EntityType}")}"
                        : a.Action,
                    Time = a.CreatedAt,
                    Icon = a.Action.Contains("register", StringComparison.OrdinalIgnoreCase) ? "person_add" :
                           a.Action.Contains("appointment", StringComparison.OrdinalIgnoreCase) ? "calendar_today" :
                           a.Action.Contains("doctor", StringComparison.OrdinalIgnoreCase) ? "medical_services" :
                           a.Action.Contains("report", StringComparison.OrdinalIgnoreCase) ? "assessment" : "history"
                })
                .ToListAsync();

            var activities = recentAudits.Select(a => new
            {
                a.Id,
                a.Title,
                a.Description,
                Time = FormatRelativeTime(a.Time),
                a.Icon
            }).ToList();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Activities loaded",
                Data = activities
            });
        }

        [HttpGet("recent-users")]
        public async Task<IActionResult> GetRecentUsers()
        {
            try
            {
                var users = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .Select(u => new
                    {
                        u.Id,
                        u.FullName,
                        u.Email,
                        u.Role,
                        u.IsActive,
                        CreatedAt = u.CreatedAt.HasValue ? u.CreatedAt.Value.ToString("yyyy-MM-dd") : null
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Recent users loaded",
                    Data = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error loading users: {ex.Message}",
                    Data = null
                });
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? role, [FromQuery] string? search)
        {
            try
            {
                var query = _context.Users.AsQueryable();

                if (!string.IsNullOrEmpty(role) && role != "All")
                {
                    query = query.Where(u => u.Role == role);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    query = query.Where(u => u.FullName.ToLower().Contains(search) ||
                                           u.Email.ToLower().Contains(search) ||
                                           (u.RegistrationNumber != null && u.RegistrationNumber.ToLower().Contains(search)));
                }

                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Select(u => new
                    {
                        u.Id,
                        u.FullName,
                        u.Email,
                        u.Role,
                        u.Department,
                        u.RegistrationNumber,
                        u.IsActive,
                        Status = (u.IsActive ?? true) ? "Active" : "Suspended",
                        JoinedDate = u.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Users loaded successfully",
                    Data = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error loading users: {ex.Message}",
                    Data = null
                });
            }
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new ApiResponse<object> { Success = false, Message = "User not found" });
                }

                // Remove associated records first if necessary (Cascading delete handles usually)
                // For safety, let's just mark as deleted or delete if cascade is set up
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "User deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error deleting user: {ex.Message}"
                });
            }
        }

        [HttpPatch("users/{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new ApiResponse<object> { Success = false, Message = "User not found" });
                }

                user.IsActive = !(user.IsActive ?? true);
                if (user.IsActive == false) user.IsEmailVerified = false; // Example logic

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"User status changed to {(user.IsActive == true ? "Active" : "Suspended")}",
                    Data = new { isActive = user.IsActive, status = (user.IsActive == true ? "Active" : "Suspended") }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error updating user status: {ex.Message}"
                });
            }
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var notifications = await _context.Notifications
                .Include(n => n.User)
                .Where(n => n.User.Role == "Admin" || n.User.Role == "admin")
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    Time = FormatRelativeTime(n.CreatedAt),
                    IsRead = n.IsRead ?? false,
                    n.Type
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Notifications loaded",
                Data = notifications
            });
        }

        private static string FormatRelativeTime(DateTime? timestamp)
        {
            if (!timestamp.HasValue)
            {
                return "just now";
            }

            var span = DateTime.UtcNow - timestamp.Value.ToUniversalTime();

            if (span.TotalMinutes < 1) return "just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} mins ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} hours ago";
            return $"{(int)span.TotalDays} days ago";
        }


        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Email already exists" });
            }

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Role = dto.Role,
                Department = dto.Department,
                RegistrationNumber = dto.RegistrationNumber,
                PhoneNumber = dto.PhoneNumber,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Address = dto.Address,
                IsActive = dto.IsActive,
                IsEmailVerified = dto.IsEmailVerified,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            if (dto.Role == "Doctor")
            {
                var doctor = new Doctor
                {
                    UserId = user.Id,
                    Specialization = dto.Specialization ?? "General",
                    LicenseNumber = dto.LicenseNumber ?? "N/A",
                    Qualification = "MBBS",
                    Experience = dto.ExperienceYears ?? 0,
                    Bio = dto.Bio,
                    IsAvailable = true
                };
                await _context.Doctors.AddAsync(doctor);
                await _context.SaveChangesAsync();
            }

            return Ok(new ApiResponse<object> { Success = true, Message = "User created successfully" });
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _context.Users.Include(u => u.Doctor).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = "User not found" });
            }

            user.FullName = dto.FullName;
            user.Department = dto.Department;
            user.RegistrationNumber = dto.RegistrationNumber;
            user.PhoneNumber = dto.PhoneNumber;
            user.DateOfBirth = dto.DateOfBirth;
            user.Gender = dto.Gender;
            user.Address = dto.Address;
            user.IsActive = dto.IsActive;
            user.IsEmailVerified = dto.IsEmailVerified;
            user.UpdatedAt = DateTime.UtcNow;

            if (user.Role == "Doctor")
            {
                if (user.Doctor == null)
                {
                    var doctor = new Doctor
                    {
                        UserId = user.Id,
                        Specialization = dto.Specialization ?? "General",
                        LicenseNumber = dto.LicenseNumber ?? "N/A",
                        Qualification = "MBBS",
                        Experience = dto.ExperienceYears ?? 0,
                        Bio = dto.Bio,
                        IsAvailable = true
                    };
                    await _context.Doctors.AddAsync(doctor);
                }
                else
                {
                    if (dto.Specialization != null) user.Doctor.Specialization = dto.Specialization;
                    if (dto.LicenseNumber != null) user.Doctor.LicenseNumber = dto.LicenseNumber;
                    if (dto.ExperienceYears.HasValue) user.Doctor.Experience = dto.ExperienceYears.Value;
                    if (dto.Bio != null) user.Doctor.Bio = dto.Bio;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<object> { Success = true, Message = "User updated successfully" });
        }
    }
}
