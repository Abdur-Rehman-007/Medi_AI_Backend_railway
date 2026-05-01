using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Medicalhistory
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public string RecordType { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly? DiagnosisDate { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User Patient { get; set; } = null!;
}
