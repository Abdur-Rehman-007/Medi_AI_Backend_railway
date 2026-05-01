using System.Net;
using System.Net.Mail;

namespace Backend_APIs.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly string _username;
        private readonly string _password;
        private readonly bool _enableSsl;
        private readonly bool _useConsoleForDevelopment;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var emailSettings = _configuration.GetSection("EmailSettings");
            _smtpHost = emailSettings["SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
            _senderEmail = emailSettings["SenderEmail"] ?? "";
            _senderName = emailSettings["SenderName"] ?? "MediAI Healthcare";
            _username = emailSettings["Username"] ?? "";
            _password = emailSettings["Password"] ?? "";
            _enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");
            _useConsoleForDevelopment = bool.Parse(emailSettings["UseConsoleForDevelopment"] ?? "false");
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                // Development mode: log to console
                if (_useConsoleForDevelopment)
                {
                    _logger.LogInformation("=================================");
                    _logger.LogInformation($"?? EMAIL (Development Mode)");
                    _logger.LogInformation($"To: {toEmail}");
                    _logger.LogInformation($"Subject: {subject}");
                    _logger.LogInformation($"Body: {htmlBody}");
                    _logger.LogInformation("=================================");
                    Console.WriteLine("=================================");
                    Console.WriteLine($"?? EMAIL SENT TO: {toEmail}");
                    Console.WriteLine($"Subject: {subject}");
                    Console.WriteLine($"Body: {htmlBody}");
                    Console.WriteLine("=================================");
                    return true;
                }

                // Production mode: send actual email
                if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
                {
                    _logger.LogWarning("Email credentials not configured. Falling back to console output.");
                    Console.WriteLine($"?? Email to {toEmail}: {subject} - {htmlBody}");
                    return false;
                }

                using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
                {
                    EnableSsl = _enableSsl,
                    Credentials = new NetworkCredential(_username, _password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_senderEmail, _senderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email to {toEmail}: {ex.Message}");
                
                // Fallback to console in case of error
                Console.WriteLine("=================================");
                Console.WriteLine($"? EMAIL FAILED - Showing in console");
                Console.WriteLine($"To: {toEmail}");
                Console.WriteLine($"Subject: {subject}");
                Console.WriteLine($"Body: {htmlBody}");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("=================================");
                
                return false;
            }
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string userName, string otp)
        {
            var subject = "Email Verification - MediAI Healthcare";
            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; color: #2563eb; margin-bottom: 30px; }}
        .otp-box {{ background-color: #eff6ff; border: 2px solid #2563eb; border-radius: 8px; padding: 20px; text-align: center; margin: 20px 0; }}
        .otp-code {{ font-size: 32px; font-weight: bold; color: #1e40af; letter-spacing: 5px; }}
        .content {{ color: #333; line-height: 1.6; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb; text-align: center; color: #6b7280; font-size: 12px; }}
        .warning {{ color: #dc2626; font-size: 14px; margin-top: 15px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>?? MediAI Healthcare</h1>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p>Thank you for registering with MediAI Healthcare! Please verify your email address using the OTP below:</p>
            
            <div class='otp-box'>
                <p style='margin: 0; font-size: 14px; color: #6b7280;'>Your Verification Code</p>
                <div class='otp-code'>{otp}</div>
                <p style='margin: 10px 0 0 0; font-size: 12px; color: #6b7280;'>Valid for 10 minutes</p>
            </div>
            
            <p>Enter this code in the verification page to complete your registration.</p>
            <p class='warning'>?? If you didn't request this code, please ignore this email.</p>
        </div>
        <div class='footer'>
            <p>© 2024 MediAI Healthcare. All rights reserved.</p>
            <p>This is an automated email. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "Welcome to MediAI Healthcare! ??";
            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; color: #2563eb; margin-bottom: 30px; }}
        .content {{ color: #333; line-height: 1.6; }}
        .button {{ display: inline-block; background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .features {{ background-color: #f9fafb; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .feature-item {{ margin: 10px 0; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb; text-align: center; color: #6b7280; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>?? Welcome to MediAI Healthcare!</h1>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p>Your email has been verified successfully! Welcome to our healthcare community.</p>
            
            <div class='features'>
                <h3>What you can do now:</h3>
                <div class='feature-item'>? Book appointments with doctors</div>
                <div class='feature-item'>?? Set up medicine reminders</div>
                <div class='feature-item'>?? Track your health records</div>
                <div class='feature-item'>????? Connect with healthcare professionals</div>
                <div class='feature-item'>?? Receive important health notifications</div>
            </div>
            
            <p>If you have any questions or need assistance, feel free to reach out to our support team.</p>
            <p><strong>Stay healthy! ??</strong></p>
        </div>
        <div class='footer'>
            <p>© 2024 MediAI Healthcare. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetToken)
        {
            var subject = "Password Reset Request - MediAI Healthcare";
            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; color: #2563eb; margin-bottom: 30px; }}
        .content {{ color: #333; line-height: 1.6; }}
        .token-box {{ background-color: #fef2f2; border: 2px solid #dc2626; border-radius: 8px; padding: 20px; text-align: center; margin: 20px 0; }}
        .token {{ font-family: monospace; font-size: 14px; color: #991b1b; word-break: break-all; }}
        .warning {{ color: #dc2626; font-size: 14px; margin-top: 15px; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #e5e7eb; text-align: center; color: #6b7280; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>?? MediAI Healthcare</h1>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p>We received a request to reset your password. Use the token below to reset your password:</p>
            
            <div class='token-box'>
                <p style='margin: 0; font-size: 14px; color: #6b7280;'>Reset Token</p>
                <div class='token'>{resetToken}</div>
                <p style='margin: 10px 0 0 0; font-size: 12px; color: #6b7280;'>Valid for 1 hour</p>
            </div>
            
            <p class='warning'>?? If you didn't request a password reset, please ignore this email and ensure your account is secure.</p>
        </div>
        <div class='footer'>
            <p>© 2024 MediAI Healthcare. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, htmlBody);
        }
    }
}
