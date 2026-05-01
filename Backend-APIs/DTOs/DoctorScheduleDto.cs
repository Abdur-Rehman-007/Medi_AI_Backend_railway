namespace Backend_APIs.DTOs
{
    /// <summary>
    /// DTO for doctor schedule
    /// </summary>
    public class DoctorScheduleDto
    {
        public int ScheduleId { get; set; }
        public string DayOfWeek { get; set; } = null!;
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;
        public bool IsAvailable { get; set; }
    }
}
