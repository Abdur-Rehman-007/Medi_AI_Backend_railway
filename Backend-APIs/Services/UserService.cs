using Backend_APIs.DTOs;
using Backend_APIs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Backend_APIs.Services
{
    public class UserService : IUserService
    {
        private readonly MediaidbContext _context;
        private readonly IWebHostEnvironment _environment;

        public UserService(MediaidbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<(bool Success, string Message, UserDto? User)> GetProfileAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return (false, "User not found", null);
                }

                var userDto = MapToUserDto(user);
                return (true, "Profile retrieved successfully", userDto);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to retrieve profile: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, UserDto? User)> UpdateProfileAsync(int userId, UpdateProfileDto updateProfileDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return (false, "User not found", null);
                }

                // Update user properties
                if (!string.IsNullOrWhiteSpace(updateProfileDto.FullName))
                    user.FullName = updateProfileDto.FullName;

                if (!string.IsNullOrWhiteSpace(updateProfileDto.PhoneNumber))
                    user.PhoneNumber = updateProfileDto.PhoneNumber;

                if (updateProfileDto.DateOfBirth.HasValue)
                    user.DateOfBirth = updateProfileDto.DateOfBirth;

                if (!string.IsNullOrWhiteSpace(updateProfileDto.Gender))
                    user.Gender = updateProfileDto.Gender;

                if (!string.IsNullOrWhiteSpace(updateProfileDto.Address))
                    user.Address = updateProfileDto.Address;

                if (!string.IsNullOrWhiteSpace(updateProfileDto.Department))
                    user.Department = updateProfileDto.Department;

                if (!string.IsNullOrWhiteSpace(updateProfileDto.RegistrationNumber))
                    user.RegistrationNumber = updateProfileDto.RegistrationNumber;

                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var userDto = MapToUserDto(user);
                return (true, "Profile updated successfully", userDto);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to update profile: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return (false, "User not found");
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
                {
                    return (false, "Current password is incorrect");
                }

                // Hash and update new password
                var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
                user.PasswordHash = newPasswordHash;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return (true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to change password: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message, string? ImageUrl)> UploadProfilePhotoAsync(int userId, IFormFile photo)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return (false, "User not found", null);
                }

                // Validate file
                if (photo == null || photo.Length == 0)
                {
                    return (false, "No file uploaded", null);
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return (false, "Invalid file type. Only JPG, PNG, and GIF are allowed", null);
                }

                // Validate file size (max 5MB)
                if (photo.Length > 5 * 1024 * 1024)
                {
                    return (false, "File size must be less than 5MB", null);
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "profiles");
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Delete old profile photo if exists
                if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
                {
                    var oldFileName = Path.GetFileName(user.ProfileImageUrl);
                    var oldFilePath = Path.Combine(uploadsPath, oldFileName);
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }

                // Save new file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                // Update user profile with new image URL
                var imageUrl = $"/uploads/profiles/{fileName}";
                user.ProfileImageUrl = imageUrl;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return (true, "Profile photo uploaded successfully", imageUrl);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to upload photo: {ex.Message}", null);
            }
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
    }
}
