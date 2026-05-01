using Backend_APIs.DTOs;
using Backend_APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend_APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Data = ModelState
                });
            }

            var (success, message) = await _authService.RegisterAsync(registerDto);

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
        /// Verify OTP and complete registration
        /// </summary>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto verifyOtpDto)
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

            var (success, message, token, user) = await _authService.VerifyOtpAsync(verifyOtpDto);

            if (!success)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = message,
                    Data = null
                });
            }

            var response = new AuthDataResponse
            {
                Token = token!,
                User = MapToUserResponseDto(user!)
            };

            return Ok(new ApiResponse<AuthDataResponse>
            {
                Success = true,
                Message = message,
                Data = response
            });
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
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

            var (success, message, token, user) = await _authService.LoginAsync(loginDto);

            if (!success)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = message,
                    Data = null
                });
            }

            var response = new AuthDataResponse
            {
                Token = token!,
                User = MapToUserResponseDto(user!)
            };

            return Ok(new ApiResponse<AuthDataResponse>
            {
                Success = true,
                Message = message,
                Data = response
            });
        }

        /// <summary>
        /// Get current authenticated user details
        /// </summary>
        [HttpGet("current-user")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
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
            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found",
                    Data = null
                });
            }

            return Ok(new ApiResponse<UserResponseDto>
            {
                Success = true,
                Message = "User retrieved successfully",
                Data = MapToUserResponseDto(user)
            });
        }

        /// <summary>
        /// Test endpoint to verify API is running
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                message = "MediAI Backend API is running"
            });
        }

        /// <summary>
        /// Request password reset — verifies email + phone number both match the same account,
        /// then sends a 6-digit OTP to the registered email address.
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Data = ModelState
                });
            }

            var (success, message, resetToken) = await _authService.ForgotPasswordAsync(forgotPasswordDto);

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
                Data = new { resetToken }
            });
        }

        /// <summary>
        /// Reset password using the OTP token received via email.
        /// On success the user can immediately log in with the new password.
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Data = ModelState
                });
            }

            var (success, message) = await _authService.ResetPasswordAsync(resetPasswordDto);

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
        /// Resend OTP for email verification (for users who did not receive / whose OTP expired).
        /// </summary>
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto resendOtpDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Data = ModelState
                });
            }

            var (success, message) = await _authService.ResendOtpAsync(resendOtpDto);

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
        /// Logout - invalidates the session on the client side
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // JWT is stateless; the client removes the token.
            // If token blacklisting is needed later, add the token to a blacklist store here.
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Logged out successfully",
                Data = null
            });
        }

        /// <summary>
        /// Map UserDto to UserResponseDto with Flutter-friendly naming
        /// </summary>
        private UserResponseDto MapToUserResponseDto(UserDto user)
        {
            return new UserResponseDto
            {
                UserId = user.Id,
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
        }
    }
}
