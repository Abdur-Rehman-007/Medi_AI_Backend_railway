using Backend_APIs.DTOs;
using Backend_APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend_APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get current user's profile
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
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
            var (success, message, user) = await _userService.GetProfileAsync(userId);

            if (!success)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = message,
                    Data = null
                });
            }

            // Map to Flutter-friendly response
            var userResponse = new UserResponseDto
            {
                UserId = user!.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Department = user.Department,
                RegistrationNumber = user.RegistrationNumber,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth?.ToString("yyyy-MM-dd"),
                Gender = user.Gender,
                Address = user.Address,
                ProfileImageUrl = user.ProfileImageUrl,
                IsEmailVerified = user.IsEmailVerified ?? false,
                IsActive = user.IsActive ?? true
            };

            return Ok(new ApiResponse<UserResponseDto>
            {
                Success = true,
                Message = message,
                Data = userResponse
            });
        }

        /// <summary>
        /// Update current user's profile
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
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
            var (success, message, user) = await _userService.UpdateProfileAsync(userId, updateProfileDto);

            if (!success)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = message,
                    Data = null
                });
            }

            // Map to Flutter-friendly response
            var userResponse = new UserResponseDto
            {
                UserId = user!.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Department = user.Department,
                RegistrationNumber = user.RegistrationNumber,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth?.ToString("yyyy-MM-dd"),
                Gender = user.Gender,
                Address = user.Address,
                ProfileImageUrl = user.ProfileImageUrl,
                IsEmailVerified = user.IsEmailVerified ?? false,
                IsActive = user.IsActive ?? true
            };

            return Ok(new ApiResponse<UserResponseDto>
            {
                Success = true,
                Message = message,
                Data = userResponse
            });
        }

        /// <summary>
        /// Change current user's password
        /// </summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
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
            var (success, message) = await _userService.ChangePasswordAsync(userId, changePasswordDto);

            if (!success)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = message,
                    Data = null
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = message,
                Data = null
            });
        }

        /// <summary>
        /// Upload profile photo
        /// </summary>
        [HttpPost("upload-photo")]
        public async Task<IActionResult> UploadPhoto([FromForm] IFormFile photo)
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
            var (success, message, imageUrl) = await _userService.UploadProfilePhotoAsync(userId, photo);

            if (!success)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = message,
                    Data = null
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = message,
                Data = new { imageUrl }
            });
        }
    }
}
