using System;
using System.Collections.Generic;

namespace Backend_APIs.Models;

public partial class Healthtip
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public string? Source { get; set; }

    public int? AuthorId { get; set; }

    public int? Views { get; set; }

    public int? Likes { get; set; }

    public bool? IsPublished { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? Author { get; set; }

    public virtual ICollection<Healthtipinteraction> Healthtipinteractions { get; set; } = new List<Healthtipinteraction>();
}
