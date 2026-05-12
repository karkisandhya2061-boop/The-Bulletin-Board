namespace WebApplication1.Models
{
    public class OpinionStory
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int ReadTimeMinutes { get; set; } = 1;
        public string Tone { get; set; } = string.Empty;
        public bool IsPinned { get; set; } = false;
        public int PinOrder { get; set; } = 0;
        public string Status { get; set; } = "draft";
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CreateOpinionStoryRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int ReadTimeMinutes { get; set; } = 1;
        public string Tone { get; set; } = string.Empty;
        public bool IsPinned { get; set; } = false;
        public int PinOrder { get; set; } = 0;
        public string Status { get; set; } = "draft";
        public DateTime? PublishedAt { get; set; }
    }

    public class UpdateOpinionStoryRequest : CreateOpinionStoryRequest { }
}