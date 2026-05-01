namespace Backend_APIs.DTOs
{
    public class HealthTipResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? Source { get; set; }
        public string? AuthorName { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }
        public DateTime PublishedAt { get; set; }
    }

    public class CreateHealthTipDto
    {
        public string Title { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? Source { get; set; }
        public bool IsPublished { get; set; } = true;
    }

    public class UpdateHealthTipDto
    {
        public string? Title { get; set; }
        public string? Category { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public string? Source { get; set; }
        public bool? IsPublished { get; set; }
    }
}
