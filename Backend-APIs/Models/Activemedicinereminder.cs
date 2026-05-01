using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Activemedicinereminder
{
    public int ReminderId { get; set; }

    public string MedicineName { get; set; } = null!;

    public string Dosage { get; set; } = null!;

    public string Frequency { get; set; } = null!;

    public string Times { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string StudentName { get; set; } = null!;

    public string StudentEmail { get; set; } = null!;
}
