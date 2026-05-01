using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Doctorperformancesummary
{
    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = null!;

    public string Specialization { get; set; } = null!;

    public decimal? AverageRating { get; set; }

    public int? TotalRatings { get; set; }

    public long TotalAppointments { get; set; }

    public long CompletedAppointments { get; set; }

    public long CancelledAppointments { get; set; }
}
