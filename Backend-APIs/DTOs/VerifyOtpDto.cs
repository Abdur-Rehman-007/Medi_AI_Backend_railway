namespace Backend_APIs.DTOs
{
    public class VerifyOtpDto
    {
        public string Email { get; set; } = null!;
        public string Otp { get; set; } = null!;
    }
}
