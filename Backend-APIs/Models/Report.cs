using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Report
{
    public int Id { get; set; }

    public string ReportType { get; set; } = null!;

    public int GeneratedBy { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string? Parameters { get; set; }

    public string? FileUrl { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual User GeneratedByNavigation { get; set; } = null!;
}
