namespace WebApplication1.Models
{
    
    public class TechStory
    {
        public int Id { get; set; }
        public string Bucket { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int ReadTimeMinutes { get; set; }
        public string Tone { get; set; } = string.Empty;
        public bool IsPinned { get; set; }
        public int PinOrder { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Grouped response for the Tech feed (mirrors the structure of /news/feed).
    /// </summary>
    public class TechFeedResponse
    {
        public TechStory? HeroStory { get; set; }
        public List<TechStory> FeaturedSideStories { get; set; } = new();
        public List<TechStory> TrendingStories { get; set; } = new();
    }
}