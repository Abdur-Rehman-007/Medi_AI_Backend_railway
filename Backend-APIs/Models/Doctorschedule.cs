using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Doctorschedule
{
    public int Id { get; set; }

    public int DoctorId { get; set; }

    public string DayOfWeek { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;
}
