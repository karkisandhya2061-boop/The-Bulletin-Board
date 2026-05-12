namespace WebApplication1.Models
{
    /// <summary>
    /// Represents a story managed via /api/v1/admin/content/stories.
    /// Separate from the legacy Story model used by NewsController.
    /// </summary>
    public class ContentStory
    {
        public int     Id         { get; set; }
        public string  Title      { get; set; } = string.Empty;
        public string? Category   { get; set; }
        public string? Summary    { get; set; }

        /// <summary>draft | published</summary>
        public string  Status     { get; set; } = "draft";

        /// <summary>Only one story can have IsHero = true and Status = published at a time.</summary>
        public bool    IsHero     { get; set; } = false;

        public bool    IsTrending { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Request body for creating or fully updating a ContentStory.
    /// </summary>
    public class ContentStoryRequest
    {
        public string  Title      { get; set; } = string.Empty;
        public string? Category   { get; set; }
        public string? Summary    { get; set; }

        /// <summary>draft | published</summary>
        public string  Status     { get; set; } = "draft";

        public bool    IsHero     { get; set; } = false;
        public bool    IsTrending { get; set; } = false;
    }
}
