using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Feedback
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Subject { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string? AdminResponse { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; }

    public DateTime? RespondedAt { get; set; }

    public virtual User User { get; set; } = null!;
}