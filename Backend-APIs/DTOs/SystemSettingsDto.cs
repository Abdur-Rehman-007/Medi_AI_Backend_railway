namespace Backend_APIs.DTOs
{
    public class SystemSettingsDto
    {
        public bool MaintenanceMode { get; set; }
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public bool AutoApproveRegistrations { get; set; }
        public bool RequireEmailVerification { get; set; }
        public bool TwoFactorAuth { get; set; }
        public int SessionTimeoutMinutes { get; set; }
        public int MaxLoginAttempts { get; set; }
        public string SystemName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string SupportEmail { get; set; } = string.Empty;
    }
}
