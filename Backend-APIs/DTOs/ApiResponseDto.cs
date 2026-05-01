namespace Backend_APIs.DTOs
{
    /// <summary>
    /// Standard API response wrapper for consistent Flutter integration
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public T? Data { get; set; }
        public object? Errors { get; set; }
    }

    /// <summary>
    /// Login/OTP verification response with token and user data
    /// </summary>
    public class AuthDataResponse
    {
        public string Token { get; set; } = null!;
        public UserResponseDto User { get; set; } = null!;
    }

    /// <summary>
    /// User data response with camelCase naming for Flutter compatibility
    /// </summary>
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? Department { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? DateOfBirth { get; set; } // String format for Flutter
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsActive { get; set; }
    }
}
