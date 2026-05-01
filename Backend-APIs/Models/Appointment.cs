using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Appointment
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public TimeOnly AppointmentTime { get; set; }

    public int? Duration { get; set; }

    public string? Status { get; set; }

    public string? Symptoms { get; set; }

    public string? Notes { get; set; }

    public string? CancellationReason { get; set; }

    public int? CancelledBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? CancelledByNavigation { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual Doctorreview? Doctorreview { get; set; }

    public virtual User Patient { get; set; } = null!;

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
