using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Medicinereminder
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public string MedicineName { get; set; } = null!;

    public string Dosage { get; set; } = null!;

    public string Frequency { get; set; } = null!;

    public string? CustomFrequency { get; set; }

    public string Times { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Notes { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Medicinereminderlog> Medicinereminderlogs { get; set; } = new List<Medicinereminderlog>();

    public virtual User Student { get; set; } = null!;
}
