using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Medicinereminderlog
{
    public int Id { get; set; }

    public int ReminderId { get; set; }

    public DateTime ScheduledTime { get; set; }

    public DateTime? TakenTime { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Medicinereminder Reminder { get; set; } = null!;
}
