using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Todaysappointment
{
    public int AppointmentId { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public TimeOnly AppointmentTime { get; set; }

    public string? Status { get; set; }

    public string? Symptoms { get; set; }

    public string PatientName { get; set; } = null!;

    public string PatientEmail { get; set; } = null!;

    public string? PatientPhone { get; set; }

    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = null!;

    public string Specialization { get; set; } = null!;
}
