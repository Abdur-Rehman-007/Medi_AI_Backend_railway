namespace Backend_APIs.DTOs
{
    /// <summary>
    /// DTO for available time slots
    /// </summary>
    public class AvailableSlotDto
    {
        public string Time { get; set; } = null!;
        public int Duration { get; set; } = 30; // Default 30 minutes
        public bool Available { get; set; }
    }

    /// <summary>
    /// DTO for available slots response
    /// </summary>
    public class AvailableSlotsResponseDto
    {
        public string Date { get; set; } = null!;
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public List<AvailableSlotDto> Slots { get; set; } = new();
    }
}
