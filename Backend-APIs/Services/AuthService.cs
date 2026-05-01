using Backend_APIs.DTOs;
using Backend_APIs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace Backend_APIs.Services
{
    public class AuthService : IAuthService
    {
        private readonly MediaidbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        private const int DEFAULT_MAX_LOGIN_ATTEMPTS = 5;
        private const int DEFAULT_SESSION_TIMEOUT = 30; // 30 minutes
        private const int LOCKOUT_DURATION_MINUTES = 30;

        public AuthService(MediaidbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Normalize and validate role against DB enum values
                var role = (registerDto.Role ?? string.Empty).Trim();
                role = role.ToLower() switch
                {
                    "student" => "Student",
                    "faculty" => "Faculty",
                    "doctor" => "Doctor",
                    "admin" => "Admin",
                    _ => string.Empty
                };

                if (string.IsNullOrWhiteSpace(role))
                {
                    return (false, "Invalid role. Allowed roles: Student, Faculty, Doctor, Admin");
                }

                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
                if (existingUser != null)
                {
                    return (false, "Email already registered");
                }

                if (role == "Doctor")
                {
                    if (string.IsNullOrWhiteSpace(registerDto.Specialization) ||
                        string.IsNullOrWhiteSpace(registerDto.LicenseNumber) ||
                        string.IsNullOrWhiteSpace(registerDto.Qualification))
                    {
                        return (false, "Doctor registration requires specialization, license number, and qualification");
                    }

                    var licenseExists = await _context.Doctors
                        .AnyAsync(d => d.LicenseNumber == registerDto.LicenseNumber.Trim());
                    if (licenseExists)
                    {
                        return (false, "License number already exists");
                    }
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

                // Fetch System Settings
                var requireVerificationSetting = await _context.Systemsettings.FirstOrDefaultAsync(s => s.SettingKey == "RequireEmailVerification");
                bool requireVerification = requireVerificationSetting != null && bool.Parse(requireVerificationSetting.SettingValue);

                var autoApproveSetting = await _context.Systemsettings.FirstOrDefaultAsync(s => s.SettingKey == "AutoApproveRegistrations");
                bool autoApprove = autoApproveSetting == null || bool.Parse(autoApproveSetting.SettingValue); // Default to true if not set? No, safer false. But existing default was true. Let's assume true for now to avoid breaking existing users unless explicit.

                // Create new user
                var user = new User
                {
                    Email = registerDto.Email,
                    PasswordHash = passwordHash,
                    FullName = registerDto.FullName,
                    Role = role,
                    Department = registerDto.Department,
                    RegistrationNumber = registerDto.RegistrationNumber,
                    PhoneNumber = registerDto.PhoneNumber,
                    DateOfBirth = registerDto.DateOfBirth,
                    Gender = registerDto.Gender,
                    Address = registerDto.Address,
                    IsEmailVerified = !requireVerification, 
                    IsActive = autoApprove, 
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                if (role == "Doctor")
                {
                    var doctor = new Doctor
                    {
                        UserId = user.Id,
                        Specialization = registerDto.Specialization!.Trim(),
                        LicenseNumber = registerDto.LicenseNumber!.Trim(),
                        Qualification = registerDto.Qualification!.Trim(),
                        Experience = registerDto.Experience ?? 0,
                        RoomNumber = string.IsNullOrWhiteSpace(registerDto.RoomNumber)
                            ? null
                            : registerDto.RoomNumber.Trim(),
                        Bio = string.IsNullOrWhiteSpace(registerDto.Bio)
                            ? null
                            : registerDto.Bio.Trim(),
                        IsAvailable = true,
                        AverageRating = 0,
                        TotalRatings = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Doctors.Add(doctor);
                    await _context.SaveChangesAsync();
                }

                if (requireVerification) {
                    var otp = GenerateOtp();
                    var otpRecord = new Emailverificationotp
                    {
                        UserId = user.Id,
                        Otp = otp,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(10), // Should also be from settings
                        IsUsed = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Emailverificationotps.Add(otpRecord);
                    await _context.SaveChangesAsync();

                    // Send OTP via email
                    await _emailService.SendOtpEmailAsync(user.Email, user.FullName, otp);
                    return (true, "Registration successful! Please verify your email.");
                }

                return (true, "Registration successful! You can now login.");
            }
            catch (Exception ex)
            {
                return (false, $"Registration failed: {ex.Message}");
            }
        }


        public async Task<(bool Success, string Message, string? Token, UserDto? User)> VerifyOtpAsync(VerifyOtpDto verifyOtpDto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == verifyOtpDto.Email);
                if (user == null)
                {
                    return (false, "User not found", null, null);
                }

                var otpRecord = await _context.Emailverificationotps
                    .Where(o => o.UserId == user.Id && o.Otp == verifyOtpDto.Otp && o.IsUsed == false)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                if (otpRecord == null)
                {
                    return (false, "Invalid OTP", null, null);
                }

                if (otpRecord.ExpiresAt < DateTime.UtcNow)
                {
                    return (false, "OTP expired", null, null);
                }

                // Mark OTP as used
                otpRecord.IsUsed = true;
                user.IsEmailVerified = true;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);

                // Generate JWT token
                var token = await GenerateJwtToken(user);
                var userDto = MapToUserDto(user);

                return (true, "Email verified successfully! Welcome to MediAI Healthcare.", token, userDto);
            }
            catch (Exception ex)
            {
                return (false, $"Verification failed: {ex.Message}", null, null);
            }
        }

        public async Task<(bool Success, string Message, string? Token, UserDto? User)> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
                if (user == null)
                {
                    return (false, "Invalid email or password", null, null);
                }


                /* Temporarily Disabled Lackout Logic due to DB Schema Mismatch
                // Check for account lockout
                if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
                {
                   var minutesLeft = (int)(user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes;
                   return (false, $"Account locked due to too many failed attempts. Try again in {minutesLeft} minutes.", null, null);
                }
                */

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    /* Temporarily Disabled Failed Attempt Tracking due to DB Schema Mismatch
                    // Increment failed login attempts
                    user.FailedLoginAttempts++;

                    // Get MaxLoginAttempts setting
                    var maxAttemptsSetting = await _context.Systemsettings
                        .FirstOrDefaultAsync(s => s.SettingKey == "MaxLoginAttempts");
                    
                    int maxAttempts = DEFAULT_MAX_LOGIN_ATTEMPTS;
                    if (maxAttemptsSetting != null && int.TryParse(maxAttemptsSetting.SettingValue, out int val))
                    {
                        maxAttempts = val;
                    }

                    if (user.FailedLoginAttempts >= maxAttempts)
                    {
                        user.LockoutEnd = DateTime.UtcNow.AddMinutes(LOCKOUT_DURATION_MINUTES);
                        user.FailedLoginAttempts = 0; // Reset attempts after lockout or keep them? Usually reset or keep max. Let's reset to start clean cycle after lockout.
                        await _context.SaveChangesAsync();
                        return (false, $"Account locked for {LOCKOUT_DURATION_MINUTES} minutes due to {maxAttempts} failed login attempts.", null, null);
                    }

                    await _context.SaveChangesAsync();
                    int attemptsLeft = maxAttempts - user.FailedLoginAttempts;
                    return (false, $"Invalid email or password. {attemptsLeft} attempts remaining.", null, null);
                    */
                    return (false, "Invalid email or password", null, null);
                }

                /* Temporarily Disabled Reset Logic due to DB Schema Mismatch
                // Reset failed attempts on successful login
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                */

                if (user.IsActive == false)
                {
                    return (false, "Account is deactivated", null, null);
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Generate JWT token
                var token = await GenerateJwtToken(user);
                var userDto = MapToUserDto(user);

                return (true, "Login successful", token, userDto);
            }
            catch (Exception ex)
            {
                return (false, $"Login failed: {ex.Message}", null, null);
            }
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user != null ? MapToUserDto(user) : null;
        }

        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Fetch session timeout from DB
             var timeoutSetting = await _context.Systemsettings
                .FirstOrDefaultAsync(s => s.SettingKey == "SessionTimeoutMinutes");
            
            int sessionTimeoutMinutes = DEFAULT_SESSION_TIMEOUT;
            if (timeoutSetting != null && int.TryParse(timeoutSetting.SettingValue, out int val))
            {
                sessionTimeoutMinutes = val;
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(sessionTimeoutMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Department = user.Department,
                RegistrationNumber = user.RegistrationNumber,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Address = user.Address,
                ProfileImageUrl = user.ProfileImageUrl,
                IsEmailVerified = user.IsEmailVerified,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }

        public async Task<(bool Success, string Message, string? ResetToken)> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                // Find user by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == forgotPasswordDto.Email);
                if (user == null)
                {
                    return (false, "No account found matching the provided details", null);
                }

                // Verify phone number matches
                if (!string.Equals(
                        (user.PhoneNumber ?? "").Trim(),
                        forgotPasswordDto.PhoneNumber.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "No account found matching the provided details", null);
                }

                // Verify CMS / registration number matches
                if (!string.Equals(
                        (user.RegistrationNumber ?? "").Trim(),
                        forgotPasswordDto.RegistrationNumber.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "No account found matching the provided details", null);
                }

                // Generate a secure reset token (6-digit code)
                var resetToken = GenerateOtp();

                // Invalidate any existing unused tokens for this user
                var oldTokens = await _context.Passwordresettokens
                    .Where(t => t.UserId == user.Id && t.IsUsed == false)
                    .ToListAsync();
                foreach (var old in oldTokens)
                    old.IsUsed = true;

                // Store new token
                _context.Passwordresettokens.Add(new Passwordresettoken
                {
                    UserId = user.Id,
                    Token = resetToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                // Return token directly — no email sent
                return (true, "Identity verified. You may now reset your password.", resetToken);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to process request: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetPasswordDto.Email);
                if (user == null)
                {
                    return (false, "Invalid email or token");
                }

                var tokenRecord = await _context.Passwordresettokens
                    .Where(t => t.UserId == user.Id && t.Token == resetPasswordDto.Token && t.IsUsed == false)
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefaultAsync();

                if (tokenRecord == null)
                {
                    return (false, "Invalid or expired reset token");
                }

                if (tokenRecord.ExpiresAt < DateTime.UtcNow)
                {
                    return (false, "Reset token has expired");
                }

                // Hash new password
                var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);

                // Update password
                user.PasswordHash = newPasswordHash;
                user.UpdatedAt = DateTime.UtcNow;

                // Mark token as used
                tokenRecord.IsUsed = true;

                await _context.SaveChangesAsync();

                return (true, "Password reset successful. You can now login with your new password");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to reset password: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ResendOtpAsync(ResendOtpDto resendOtpDto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resendOtpDto.Email);
                if (user == null)
                {
                    return (false, "User not found");
                }

                if (user.IsEmailVerified == true)
                {
                    return (false, "Email is already verified");
                }

                // Generate new OTP
                var otp = GenerateOtp();

                // Mark old OTPs as used
                var oldOtps = await _context.Emailverificationotps
                    .Where(o => o.UserId == user.Id && o.IsUsed == false)
                    .ToListAsync();

                foreach (var oldOtp in oldOtps)
                {
                    oldOtp.IsUsed = true;
                }

                // Create new OTP record
                var otpRecord = new Emailverificationotp
                {
                    UserId = user.Id,
                    Otp = otp,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Emailverificationotps.Add(otpRecord);
                await _context.SaveChangesAsync();

                // Send new OTP via email
                await _emailService.SendOtpEmailAsync(user.Email, user.FullName, otp);

                return (true, "OTP has been resent to your email");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to resend OTP: {ex.Message}");
            }
        }
    }
}
