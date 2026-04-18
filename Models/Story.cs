namespace WebApplication1.Models
{
    // Represents a full news story stored in the database
    public class Story
    {
        public int Id { get; set; }
        // Content bucket this story belongs to
        public string Bucket { get; set; } = "featuredSideStories"; // heroStory | featuredSideStories | trendingStories | tickerItems
        public string Category { get; set; } = string.Empty;   // category / tag
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;    // body summary / excerpt
        public string Author { get; set; } = string.Empty;
        public int ReadTimeMinutes { get; set; } = 1;          // estimated read time in minutes
        public string Tone { get; set; } = string.Empty;       // tone / theme token for styling
        // Pinning — used for trendingStories ordering (pinned first, then latest updated_at)
        public bool IsPinned { get; set; } = false;
        public int PinOrder { get; set; } = 0;                 // lower value = higher position in list
        // Publication status: draft | published | archived
        public string Status { get; set; } = "draft";
        public DateTime? PublishedAt { get; set; }             // null if not yet published
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // Minimal model for ticker items — short text-only news entries shown in the ticker bar
    public class TickerItem
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Status { get; set; } = "published";
        public int SortOrder { get; set; } = 0;               // controls display order in ticker
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // DTO used when creating a new story via the API
    public class CreateStoryRequest
    {
        public string Bucket { get; set; } = "featuredSideStories";
        public string Category { get; set; } = string.Empty;
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

    // DTO used when updating an existing story — inherits all fields from CreateStoryRequest
    public class UpdateStoryRequest : CreateStoryRequest { }

    // DTO used when creating a new ticker item via the API
    public class CreateTickerItemRequest
    {
        public string Text { get; set; } = string.Empty;
        public string Status { get; set; } = "published";
        public int SortOrder { get; set; } = 0;
    }

    // DTO used when updating an existing ticker item — inherits all fields from CreateTickerItemRequest
    public class UpdateTickerItemRequest : CreateTickerItemRequest { }
}