namespace Backend_APIs.DTOs
{
    public class DoctorBookingSettingsDto
    {
        public int AppointmentDuration { get; set; } = 30;
        public int MaxPatientsPerDay { get; set; } = 16;
        public bool AutoConfirmAppointments { get; set; } = false;
        public bool EnableBreakTime { get; set; } = false;
        public string BreakStartTime { get; set; } = "12:00";
        public string BreakEndTime { get; set; } = "13:00";
        public bool EnableAppointmentReminders { get; set; } = true;
        public bool EnableMedicineReminders { get; set; } = true;
        public int ReminderNotificationMinutes { get; set; } = 15;
    }
}
