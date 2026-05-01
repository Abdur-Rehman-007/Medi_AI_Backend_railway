using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Healthtipinteraction
{
    public int Id { get; set; }

    public int HealthTipId { get; set; }

    public int UserId { get; set; }

    public bool? IsLiked { get; set; }

    public bool? IsBookmarked { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Healthtip HealthTip { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
