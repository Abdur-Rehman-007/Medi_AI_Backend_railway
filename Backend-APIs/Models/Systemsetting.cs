using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Systemsetting
{
    public int Id { get; set; }

    public string SettingKey { get; set; } = null!;

    public string SettingValue { get; set; } = null!;

    public string? Description { get; set; }

    public string? DataType { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}
