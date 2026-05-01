using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Symptomcheck
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Symptoms { get; set; } = null!;

    public string? Duration { get; set; }

    public string? Severity { get; set; }

    public string? Airesponse { get; set; }

    public decimal? Confidence { get; set; }

    public string? RecommendedAction { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
