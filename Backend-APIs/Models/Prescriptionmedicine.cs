using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Prescriptionmedicine
{
    public int Id { get; set; }

    public int PrescriptionId { get; set; }

    public string MedicineName { get; set; } = null!;

    public string Dosage { get; set; } = null!;

    public string Frequency { get; set; } = null!;

    public string Duration { get; set; } = null!;

    public string? Instructions { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Prescription Prescription { get; set; } = null!;
}
