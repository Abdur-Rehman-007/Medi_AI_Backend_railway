using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Doctorreview
{
    public int Id { get; set; }

    public int DoctorId { get; set; }

    public int PatientId { get; set; }

    public int AppointmentId { get; set; }

    public int Rating { get; set; }

    public string? Review { get; set; }

    public bool? IsAnonymous { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual User Patient { get; set; } = null!;
}
