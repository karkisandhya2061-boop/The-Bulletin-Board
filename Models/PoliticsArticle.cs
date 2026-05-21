namespace WebApplication1.Models
{
    // Represents a politics article stored in the database
    public class PoliticsArticle
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;       // short summary / body preview
        public string Content { get; set; } = string.Empty;       // full article body
        public string Author { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;      // optional hero image
        public string Tags { get; set; } = string.Empty;          // comma-separated tags
        public int ReadTimeMinutes { get; set; } = 1;
        public bool IsFeatured { get; set; } = false;             // promoted to top of politics feed
        public bool IsPinned { get; set; } = false;
        public int PinOrder { get; set; } = 0;                    // lower = higher position
        // Publication status: draft | published | archived
        public string Status { get; set; } = "draft";
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // DTO for creating a new politics article
    public class CreatePoliticsArticleRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public int ReadTimeMinutes { get; set; } = 1;
        public bool IsFeatured { get; set; } = false;
        public bool IsPinned { get; set; } = false;
        public int PinOrder { get; set; } = 0;
        public string Status { get; set; } = "draft";
        public DateTime? PublishedAt { get; set; }
    }

    // DTO for updating an existing politics article — same fields as create
    public class UpdatePoliticsArticleRequest : CreatePoliticsArticleRequest { }
}