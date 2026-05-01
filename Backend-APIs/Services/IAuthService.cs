using Backend_APIs.DTOs;

namespace Backend_APIs.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> RegisterAsync(RegisterDto registerDto);
        Task<(bool Success, string Message, string? Token, UserDto? User)> VerifyOtpAsync(VerifyOtpDto verifyOtpDto);
        Task<(bool Success, string Message, string? Token, UserDto? User)> LoginAsync(LoginDto loginDto);
        Task<UserDto?> GetUserByIdAsync(int userId);

        // Password Reset
        Task<(bool Success, string Message, string? ResetToken)> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);

        // OTP Management
        Task<(bool Success, string Message)> ResendOtpAsync(ResendOtpDto resendOtpDto);
    }
}
