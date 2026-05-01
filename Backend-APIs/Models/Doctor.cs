using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Doctor
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Specialization { get; set; } = null!;

    public string LicenseNumber { get; set; } = null!;

    public string Qualification { get; set; } = null!;

    public int? Experience { get; set; }

    public string? RoomNumber { get; set; }

    public string? Bio { get; set; }

    public decimal? AverageRating { get; set; }

    public int? TotalRatings { get; set; }

    public bool? IsAvailable { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Doctorleaf> Doctorleaves { get; set; } = new List<Doctorleaf>();

    public virtual ICollection<Doctorreview> Doctorreviews { get; set; } = new List<Doctorreview>();

    public virtual ICollection<Doctorschedule> Doctorschedules { get; set; } = new List<Doctorschedule>();

    public virtual User User { get; set; } = null!;
}
