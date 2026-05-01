using System.Collections.Generic;

namespace Backend_APIs.DTOs
{
    public class UpdateScheduleDto
    {
        public List<DayScheduleDto> Schedules { get; set; } = new List<DayScheduleDto>();
    }

    public class DayScheduleDto
    {
        public string DayOfWeek { get; set; } = string.Empty;
        public string StartTime { get; set; } = "09:00"; // HH:mm
        public string EndTime { get; set; } = "17:00";   // HH:mm
        public bool IsAvailable { get; set; }
    }
}