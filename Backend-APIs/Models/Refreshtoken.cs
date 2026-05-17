using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Refreshtoken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool? IsRevoked { get; set; }

    public string? ReplacedByToken { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
