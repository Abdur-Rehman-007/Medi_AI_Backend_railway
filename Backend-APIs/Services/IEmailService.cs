namespace Backend_APIs.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task<bool> SendOtpEmailAsync(string toEmail, string userName, string otp);
        Task<bool> SendWelcomeEmailAsync(string toEmail, string userName);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetToken);
    }
}
