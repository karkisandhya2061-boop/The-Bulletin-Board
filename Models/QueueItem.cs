namespace WebApplication1.Models
{
    public class QueueItem
    {
        public int    Id        { get; set; }
        public string Title     { get; set; } = string.Empty;
        public string Priority  { get; set; } = "normal";   // low | normal | high
        public string Status    { get; set; } = "pending";  // pending | inReview | completed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class QueueItemRequest
    {
        public string Title    { get; set; } = string.Empty;
        public string Priority { get; set; } = "normal";
        public string Status   { get; set; } = "pending";
    }
}
