using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Prescription
{
    public int Id { get; set; }

    public int AppointmentId { get; set; }

    public string Diagnosis { get; set; } = null!;

    public string? Notes { get; set; }

    public DateOnly? FollowUpDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual ICollection<Prescriptionmedicine> Prescriptionmedicines { get; set; } = new List<Prescriptionmedicine>();
}
