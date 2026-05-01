using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Emergencycontact
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string ContactName { get; set; } = null!;

    public string Relationship { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public bool? IsPrimary { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
