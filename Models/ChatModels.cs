namespace WebApplication1.Models
{
    /// <summary>Stored chat session — one per user conversation thread.</summary>
    public class ChatConversation
    {
        public int Id { get; set; }
        public int? UserId { get; set; }   // null for anonymous guests
        public string SessionId { get; set; } = string.Empty; // client-generated UUID
        public string Title { get; set; } = "New conversation";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>Individual message inside a conversation.</summary>
    public class ChatMessage
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public string Role { get; set; } = "user"; // "user" | "assistant"
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ─── Request / Response DTOs ──────────────────────────────────

    /// <summary>Sent by the client to start or continue a chat.</summary>
    public class ChatRequest
    {
        /// <summary>
        /// Client-generated UUID that identifies this conversation session.
        /// If a conversation with this session_id already exists it is reused;
        /// otherwise a new one is created.
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>The user's new message.</summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>Returned after each chat turn.</summary>
    public class ChatResponse
    {
        public int ConversationId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string Reply { get; set; } = string.Empty;
        public List<ChatMessage> History { get; set; } = new();
    }
}