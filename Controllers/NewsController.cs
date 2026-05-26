using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("news")]
    public class NewsController : ControllerBase
    {
        private readonly IConfiguration _config;

        private static readonly string[] ValidBuckets =
            { "heroStory", "featuredSideStories", "trendingStories", "tickerItems" };

        private static readonly string[] ValidStatuses =
            { "draft", "published", "archived" };

        public NewsController(IConfiguration config)
        {
            _config = config;
        }
        // Return grouped hero, featured, and trending stories for the homepage
        [HttpGet("feed")]
        public IActionResult GetFeed()
        {
            using var conn = OpenConnection();

            var allStories = QueryStories(conn,
                where: "status = 'published'",
                parameters: null,
                orderBy: "published_at DESC, created_at DESC");

            var tickers = QueryTickers(conn, "status = 'published'");

            var hero = allStories
                .Where(s => s.Bucket == "heroStory")
                .OrderByDescending(s => s.PublishedAt)
                .FirstOrDefault();

            var featured = allStories
                .Where(s => s.Bucket == "featuredSideStories")
                .OrderByDescending(s => s.PublishedAt)
                .ToList();

            var trending = allStories
                .Where(s => s.Bucket == "trendingStories")
                .OrderBy(s => s.IsPinned ? 0 : 1)
                .ThenBy(s => s.IsPinned ? s.PinOrder : int.MaxValue)
                .ThenByDescending(s => s.UpdatedAt)
                .ToList();

            return Ok(new
            {
                heroStory = hero,
                featuredSideStories = featured,
                trendingStories = trending,
                tickerItems = tickers
            });
        }
        // Return all published stories with optional filters
        [HttpGet("stories")]
        public IActionResult GetStories(
            [FromQuery] string? bucket = null,
            [FromQuery] string? category = null)
        {
            using var conn = OpenConnection();

            var conditions = new List<string> { "status = 'published'" };
            var parameters = new Dictionary<string, object?>();

            if (!string.IsNullOrWhiteSpace(bucket))
            {
                conditions.Add("bucket = @Bucket");
                parameters["@Bucket"] = bucket;
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                conditions.Add("category = @Category");
                parameters["@Category"] = category;
            }

            string where = string.Join(" AND ", conditions);
            string orderBy = bucket == "trendingStories"
                ? "is_pinned DESC, pin_order ASC, updated_at DESC"
                : "published_at DESC, created_at DESC";

            return Ok(QueryStories(conn, where, parameters, orderBy));
        }
        // Return a single published story by ID
        [HttpGet("stories/{id:int}")]
        public IActionResult GetStory(int id)
        {
            using var conn = OpenConnection();
            var story = GetStoryById(conn, id);

            if (story == null || story.Status != "published")
                return NotFound(new { message = "Story not found." });

            return Ok(story);
        }
        // Return published ticker items
        [HttpGet("ticker")]
        public IActionResult GetTicker()
        {
            using var conn = OpenConnection();
            return Ok(QueryTickers(conn, "status = 'published'"));
        }
        [Authorize(Roles = "admin")]
        [HttpGet("admin/stories")]
        public IActionResult AdminGetStories(
            [FromQuery] string? bucket = null,
            [FromQuery] string? status = null,
            [FromQuery] string? category = null)
        {
            using var conn = OpenConnection();

            var conditions = new List<string>();
            var parameters = new Dictionary<string, object?>();

            if (!string.IsNullOrWhiteSpace(status))
            {
                conditions.Add("status = @Status");
                parameters["@Status"] = status;
            }
            if (!string.IsNullOrWhiteSpace(bucket))
            {
                conditions.Add("bucket = @Bucket");
                parameters["@Bucket"] = bucket;
            }
            if (!string.IsNullOrWhiteSpace(category))
            {
                conditions.Add("category = @Category");
                parameters["@Category"] = category;
            }

            string where = conditions.Count > 0
                ? string.Join(" AND ", conditions)
                : "1=1";

            return Ok(QueryStories(conn, where, parameters,
                orderBy: "is_pinned DESC, pin_order ASC, updated_at DESC"));
        }
        [Authorize(Roles = "admin")]
        [HttpGet("admin/stories/{id:int}")]
        public IActionResult AdminGetStory(int id)
        {
            using var conn = OpenConnection();
            var story = GetStoryById(conn, id);
            if (story == null) return NotFound(new { message = "Story not found." });
            return Ok(story);
        }
        // Admin: create a new story
        [Authorize(Roles = "admin")]
        [HttpPost("admin/stories")]
        public IActionResult CreateStory([FromBody] CreateStoryRequest req)
        {
            if (!ValidBuckets.Contains(req.Bucket))
                return BadRequest(new { message = $"Invalid bucket. Must be one of: {string.Join(", ", ValidBuckets)}" });
            if (!ValidStatuses.Contains(req.Status))
                return BadRequest(new { message = $"Invalid status. Must be one of: {string.Join(", ", ValidStatuses)}" });

            using var conn = OpenConnection();
            if (req.Bucket == "heroStory" && req.Status == "published")
                ArchiveExistingHero(conn);

            var now = DateTime.UtcNow;
            var cmd = new MySqlCommand(@"
                INSERT INTO stories
                    (bucket, category, title, excerpt, content, author, read_time_minutes,
                     tone, is_pinned, pin_order, status, published_at, created_at, updated_at)
                VALUES
                    (@Bucket, @Category, @Title, @Excerpt, @Content, @Author, @ReadTime,
                     @Tone, @IsPinned, @PinOrder, @Status, @PublishedAt, @Now, @Now);
                SELECT LAST_INSERT_ID();", conn);

            BindStoryParams(cmd, req, now);
            var newId = Convert.ToInt32(cmd.ExecuteScalar());
            return CreatedAtAction(nameof(AdminGetStory), new { id = newId }, GetStoryById(conn, newId));
        }
        // Admin: update an existing story
        [Authorize(Roles = "admin")]
        [HttpPut("admin/stories/{id:int}")]
        public IActionResult UpdateStory(int id, [FromBody] UpdateStoryRequest req)
        {
            if (!ValidBuckets.Contains(req.Bucket))
                return BadRequest(new { message = $"Invalid bucket. Must be one of: {string.Join(", ", ValidBuckets)}" });
            if (!ValidStatuses.Contains(req.Status))
                return BadRequest(new { message = $"Invalid status. Must be one of: {string.Join(", ", ValidStatuses)}" });

            using var conn = OpenConnection();
            if (GetStoryById(conn, id) == null) return NotFound(new { message = "Story not found." });

            if (req.Bucket == "heroStory" && req.Status == "published")
                ArchiveExistingHero(conn, excludeId: id);

            var now = DateTime.UtcNow;
            var cmd = new MySqlCommand(@"
                UPDATE stories SET
                    bucket            = @Bucket,
                    category          = @Category,
                    title             = @Title,
                    excerpt           = @Excerpt,
                    content           = @Content,
                    author            = @Author,
                    read_time_minutes = @ReadTime,
                    tone              = @Tone,
                    is_pinned         = @IsPinned,
                    pin_order         = @PinOrder,
                    status            = @Status,
                    published_at      = @PublishedAt,
                    updated_at        = @Now
                WHERE id = @Id", conn);

            BindStoryParams(cmd, req, now);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();

            return Ok(GetStoryById(conn, id));
        }
        [Authorize(Roles = "admin")]
        [HttpPatch("admin/stories/{id:int}/status")]
        public IActionResult PatchStoryStatus(int id, [FromBody] PatchStatusRequest req)
        {
            if (!ValidStatuses.Contains(req.Status))
                return BadRequest(new { message = $"Invalid status. Must be one of: {string.Join(", ", ValidStatuses)}" });

            using var conn = OpenConnection();
            var existing = GetStoryById(conn, id);
            if (existing == null) return NotFound(new { message = "Story not found." });

            if (existing.Bucket == "heroStory" && req.Status == "published")
                ArchiveExistingHero(conn, excludeId: id);

            var now = DateTime.UtcNow;
            var pubAt = req.Status == "published" ? (existing.PublishedAt ?? now) : existing.PublishedAt;

            var cmd = new MySqlCommand(@"
                UPDATE stories
                SET status = @Status, published_at = @PublishedAt, updated_at = @Now
                WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@Status", req.Status);
            cmd.Parameters.AddWithValue("@PublishedAt", (object?)pubAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Now", now);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();

            return Ok(GetStoryById(conn, id));
        }
        [Authorize(Roles = "admin")]
        [HttpPatch("admin/stories/{id:int}/pin")]
        public IActionResult PatchStoryPin(int id, [FromBody] PatchPinRequest req)
        {
            using var conn = OpenConnection();
            if (GetStoryById(conn, id) == null)
                return NotFound(new { message = "Story not found." });

            var cmd = new MySqlCommand(@"
                UPDATE stories
                SET is_pinned = @IsPinned, pin_order = @PinOrder, updated_at = @Now
                WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@IsPinned", req.IsPinned);
            cmd.Parameters.AddWithValue("@PinOrder", req.PinOrder);
            cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();

            return Ok(GetStoryById(conn, id));
        }
        // Admin: delete a story
        [Authorize(Roles = "admin")]
        [HttpDelete("admin/stories/{id:int}")]
        public IActionResult DeleteStory(int id)
        {
            using var conn = OpenConnection();
            if (GetStoryById(conn, id) == null)
                return NotFound(new { message = "Story not found." });

            var cmd = new MySqlCommand("DELETE FROM stories WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Story deleted." });
        }
        [Authorize(Roles = "admin")]
        [HttpGet("admin/ticker")]
        public IActionResult AdminGetTicker([FromQuery] string? status = null)
        {
            using var conn = OpenConnection();

            var cmd = new MySqlCommand(string.IsNullOrWhiteSpace(status)
                ? "SELECT id, text, status, sort_order, created_at, updated_at FROM ticker_items ORDER BY sort_order ASC, created_at ASC"
                : "SELECT id, text, status, sort_order, created_at, updated_at FROM ticker_items WHERE status = @Status ORDER BY sort_order ASC, created_at ASC",
                conn);

            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@Status", status);

            var list = new List<TickerItem>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapTicker(reader));
            return Ok(list);
        }
        [Authorize(Roles = "admin")]
        [HttpPost("admin/ticker")]
        public IActionResult CreateTicker([FromBody] CreateTickerItemRequest req)
        {
            using var conn = OpenConnection();
            var now = DateTime.UtcNow;
            var cmd = new MySqlCommand(@"
                INSERT INTO ticker_items (text, status, sort_order, created_at, updated_at)
                VALUES (@Text, @Status, @SortOrder, @Now, @Now);
                SELECT LAST_INSERT_ID();", conn);
            cmd.Parameters.AddWithValue("@Text", req.Text);
            cmd.Parameters.AddWithValue("@Status", req.Status);
            cmd.Parameters.AddWithValue("@SortOrder", req.SortOrder);
            cmd.Parameters.AddWithValue("@Now", now);
            var newId = Convert.ToInt32(cmd.ExecuteScalar());
            return Ok(GetTickerById(conn, newId));
        }
        [Authorize(Roles = "admin")]
        [HttpPut("admin/ticker/{id:int}")]
        public IActionResult UpdateTicker(int id, [FromBody] UpdateTickerItemRequest req)
        {
            using var conn = OpenConnection();
            if (GetTickerById(conn, id) == null)
                return NotFound(new { message = "Ticker item not found." });

            var cmd = new MySqlCommand(@"
                UPDATE ticker_items
                SET text = @Text, status = @Status, sort_order = @SortOrder, updated_at = @Now
                WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@Text", req.Text);
            cmd.Parameters.AddWithValue("@Status", req.Status);
            cmd.Parameters.AddWithValue("@SortOrder", req.SortOrder);
            cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
            return Ok(GetTickerById(conn, id));
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("admin/ticker/{id:int}")]
        public IActionResult DeleteTicker(int id)
        {
            using var conn = OpenConnection();
            if (GetTickerById(conn, id) == null)
                return NotFound(new { message = "Ticker item not found." });

            var cmd = new MySqlCommand("DELETE FROM ticker_items WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Ticker item deleted." });
        }

        private MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            return conn;
        }

        private static List<Story> QueryStories(
            MySqlConnection conn,
            string where,
            Dictionary<string, object?>? parameters,
            string orderBy = "published_at DESC, created_at DESC")
        {
            var cmd = new MySqlCommand($@"
                SELECT id, bucket, category, title, excerpt, content, author,
                       read_time_minutes, tone, is_pinned, pin_order,
                       status, published_at, created_at, updated_at
                FROM stories
                WHERE {where}
                ORDER BY {orderBy}", conn);

            if (parameters != null)
                foreach (var (k, v) in parameters)
                    cmd.Parameters.AddWithValue(k, v ?? DBNull.Value);

            var list = new List<Story>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapStory(reader));
            return list;
        }

        private static List<TickerItem> QueryTickers(MySqlConnection conn, string where)
        {
            var cmd = new MySqlCommand($@"
                SELECT id, text, status, sort_order, created_at, updated_at
                FROM ticker_items
                WHERE {where}
                ORDER BY sort_order ASC, created_at ASC", conn);

            var list = new List<TickerItem>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapTicker(reader));
            return list;
        }

        private static Story? GetStoryById(MySqlConnection conn, int id)
        {
            var cmd = new MySqlCommand(@"
                SELECT id, bucket, category, title, excerpt, content, author,
                       read_time_minutes, tone, is_pinned, pin_order,
                       status, published_at, created_at, updated_at
                FROM stories WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapStory(reader) : null;
        }

        private static TickerItem? GetTickerById(MySqlConnection conn, int id)
        {
            var cmd = new MySqlCommand(@"
                SELECT id, text, status, sort_order, created_at, updated_at
                FROM ticker_items WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapTicker(reader) : null;
        }

        private static void ArchiveExistingHero(MySqlConnection conn, int excludeId = 0)
        {
            var cmd = new MySqlCommand(@"
                UPDATE stories
                SET status = 'archived', updated_at = @Now
                WHERE bucket = 'heroStory' AND status = 'published' AND id != @ExcludeId", conn);
            cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@ExcludeId", excludeId);
            cmd.ExecuteNonQuery();
        }

        private static void BindStoryParams(MySqlCommand cmd, CreateStoryRequest req, DateTime now)
        {
            cmd.Parameters.AddWithValue("@Bucket", req.Bucket);
            cmd.Parameters.AddWithValue("@Category", req.Category);
            cmd.Parameters.AddWithValue("@Title", req.Title);
            cmd.Parameters.AddWithValue("@Excerpt", req.Excerpt);
            cmd.Parameters.AddWithValue("@Content", req.Content);
            cmd.Parameters.AddWithValue("@Author", req.Author);
            cmd.Parameters.AddWithValue("@ReadTime", req.ReadTimeMinutes);
            cmd.Parameters.AddWithValue("@Tone", req.Tone);
            cmd.Parameters.AddWithValue("@IsPinned", req.IsPinned);
            cmd.Parameters.AddWithValue("@PinOrder", req.PinOrder);
            cmd.Parameters.AddWithValue("@Status", req.Status);
            cmd.Parameters.AddWithValue("@PublishedAt", (object?)req.PublishedAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Now", now);
        }

        private static Story MapStory(MySqlDataReader r) => new Story
        {
            Id = r.GetInt32("id"),
            Bucket = r.GetString("bucket"),
            Category = r.IsDBNull(r.GetOrdinal("category")) ? "" : r.GetString("category"),
            Title = r.GetString("title"),
            Excerpt = r.IsDBNull(r.GetOrdinal("excerpt")) ? "" : r.GetString("excerpt"),
            Content = r.IsDBNull(r.GetOrdinal("content")) ? "" : r.GetString("content"),
            Author = r.IsDBNull(r.GetOrdinal("author")) ? "" : r.GetString("author"),
            ReadTimeMinutes = r.GetInt32("read_time_minutes"),
            Tone = r.IsDBNull(r.GetOrdinal("tone")) ? "" : r.GetString("tone"),
            IsPinned = r.GetBoolean("is_pinned"),
            PinOrder = r.GetInt32("pin_order"),
            Status = r.GetString("status"),
            PublishedAt = r.IsDBNull(r.GetOrdinal("published_at")) ? null : r.GetDateTime("published_at"),
            CreatedAt = r.GetDateTime("created_at"),
            UpdatedAt = r.GetDateTime("updated_at"),
        };

        private static TickerItem MapTicker(MySqlDataReader r) => new TickerItem
        {
            Id = r.GetInt32("id"),
            Text = r.GetString("text"),
            Status = r.GetString("status"),
            SortOrder = r.GetInt32("sort_order"),
            CreatedAt = r.GetDateTime("created_at"),
            UpdatedAt = r.GetDateTime("updated_at"),
        };
    }
    public record PatchStatusRequest(string Status);
    public record PatchPinRequest(bool IsPinned, int PinOrder);
    public record ReactionRequest(int? UserId, string UserName, string ReactionType);
    public record CommentRequest(int? UserId, string UserName, string CommentText);
}
namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("news")]
    public class EngagementController : ControllerBase
    {
        private readonly IConfiguration _config;

        public EngagementController(IConfiguration config)
        {
            _config = config;
        }

        private MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            return conn;
        }
        [HttpPost("{id:int}/reactions")]
        public IActionResult AddReaction(int id, [FromBody] ReactionRequest request)
        {
            using var conn = OpenConnection();

            var checkCmd = new MySqlCommand("SELECT id FROM stories WHERE id = @id", conn);
            checkCmd.Parameters.AddWithValue("@id", id);
            if (checkCmd.ExecuteScalar() == null)
                return NotFound(new { message = "Story not found.", success = false });

            var userId = request.UserId ?? 0;

            // Bug 2 fix: anonymous users (userId=0) are identified by userName only to avoid collisions
            string existingReactionQuery = userId == 0
                ? "SELECT reaction_type FROM news_reactions WHERE news_article_id = @newsId AND user_name = @userName AND user_id = 0 LIMIT 1"
                : "SELECT reaction_type FROM news_reactions WHERE news_article_id = @newsId AND user_id = @userId LIMIT 1";

            string? existingReaction = null;
            var existingCmd = new MySqlCommand(existingReactionQuery, conn);
            existingCmd.Parameters.AddWithValue("@newsId", id);
            existingCmd.Parameters.AddWithValue("@userId", userId);
            existingCmd.Parameters.AddWithValue("@userName", request.UserName ?? "");
            using (var r = existingCmd.ExecuteReader())
                if (r.Read()) existingReaction = r.GetString(0);

            if (existingReaction == request.ReactionType)
                return Ok(new { message = "Reaction already set.", success = true });

            if (existingReaction == null)
            {
                var insertCmd = new MySqlCommand(
                    "INSERT INTO news_reactions (news_article_id, user_id, reaction_type) VALUES (@newsId, @userId, @reactionType)", conn);
                insertCmd.Parameters.AddWithValue("@newsId", id);
                insertCmd.Parameters.AddWithValue("@userId", userId);
                insertCmd.Parameters.AddWithValue("@reactionType", request.ReactionType);
                insertCmd.ExecuteNonQuery();

                // Notify admin
                try
                {
                    var notifCmd = new MySqlCommand(
                        "INSERT INTO admin_notifications (news_article_id, user_id, notification_type, user_name, action_text) VALUES (@newsId, @userId, 'reaction', @userName, @actionText)", conn);
                    notifCmd.Parameters.AddWithValue("@newsId", id);
                    notifCmd.Parameters.AddWithValue("@userId", userId);
                    notifCmd.Parameters.AddWithValue("@userName", request.UserName ?? "User");
                    notifCmd.Parameters.AddWithValue("@actionText", $"{request.UserName ?? "User"} has reacted");
                    notifCmd.ExecuteNonQuery();
                }
                catch { /* notifications table may not exist yet */ }
            }
            else
            {
                var updateCmd = new MySqlCommand(
                    "UPDATE news_reactions SET reaction_type = @reactionType WHERE news_article_id = @newsId AND user_id = @userId", conn);
                updateCmd.Parameters.AddWithValue("@newsId", id);
                updateCmd.Parameters.AddWithValue("@userId", userId);
                updateCmd.Parameters.AddWithValue("@reactionType", request.ReactionType);
                updateCmd.ExecuteNonQuery();
            }

            var countCmd = new MySqlCommand(
                "SELECT reaction_type, COUNT(*) as count FROM news_reactions WHERE news_article_id = @newsId GROUP BY reaction_type", conn);
            countCmd.Parameters.AddWithValue("@newsId", id);
            var reactions = new Dictionary<string, int>();
            using var reader = countCmd.ExecuteReader();
            while (reader.Read())
                reactions[reader.GetString("reaction_type")] = Convert.ToInt32(reader["count"]);

            return Ok(new { message = "Reaction added.", success = true, reactions });
        }
        // Get all reactions for a story
        [HttpGet("{id:int}/reactions")]
        public IActionResult GetReactions(int id)
        {
            using var conn = OpenConnection();
            var cmd = new MySqlCommand(
                "SELECT reaction_type, COUNT(*) as count FROM news_reactions WHERE news_article_id = @newsId GROUP BY reaction_type", conn);
            cmd.Parameters.AddWithValue("@newsId", id);
            var reactions = new Dictionary<string, int>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                reactions[reader.GetString("reaction_type")] = Convert.ToInt32(reader["count"]);
            return Ok(new { reactions, success = true });
        }
        // Post a comment on a story
        [HttpPost("{id:int}/comments")]
        public IActionResult AddComment(int id, [FromBody] CommentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.CommentText))
                return BadRequest(new { message = "Comment text is required.", success = false });

            using var conn = OpenConnection();

            var checkCmd = new MySqlCommand("SELECT id FROM stories WHERE id = @id", conn);
            checkCmd.Parameters.AddWithValue("@id", id);
            if (checkCmd.ExecuteScalar() == null)
                return NotFound(new { message = "Story not found.", success = false });

            var insertCmd = new MySqlCommand(
                "INSERT INTO news_comments (news_article_id, user_id, user_name, comment_text) VALUES (@newsId, @userId, @userName, @commentText)", conn);
            insertCmd.Parameters.AddWithValue("@newsId", id);
            insertCmd.Parameters.AddWithValue("@userId", request.UserId ?? 0);
            insertCmd.Parameters.AddWithValue("@userName", request.UserName ?? "Anonymous");
            insertCmd.Parameters.AddWithValue("@commentText", request.CommentText);
            insertCmd.ExecuteNonQuery();

            try
            {
                var notifCmd = new MySqlCommand(
                    "INSERT INTO admin_notifications (news_article_id, user_id, notification_type, user_name, action_text) VALUES (@newsId, @userId, 'comment', @userName, @actionText)", conn);
                notifCmd.Parameters.AddWithValue("@newsId", id);
                notifCmd.Parameters.AddWithValue("@userId", request.UserId ?? 0);
                notifCmd.Parameters.AddWithValue("@userName", request.UserName ?? "Anonymous");
                notifCmd.Parameters.AddWithValue("@actionText", $"{request.UserName ?? "Anonymous"} commented: {request.CommentText[..Math.Min(50, request.CommentText.Length)]}");
                notifCmd.ExecuteNonQuery();
            }
            catch { /* notifications table may not exist yet */ }

            return Ok(new { message = "Comment added.", success = true });
        }
        // Get all comments for a story
        [HttpGet("{id:int}/comments")]
        public IActionResult GetComments(int id)
        {
            using var conn = OpenConnection();
            var cmd = new MySqlCommand(
                "SELECT id, user_name, comment_text, created_at FROM news_comments WHERE news_article_id = @newsId ORDER BY created_at DESC LIMIT 50", conn);
            cmd.Parameters.AddWithValue("@newsId", id);
            var comments = new List<object>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                comments.Add(new
                {
                    id = reader.GetInt32("id"),
                    userName = reader.GetString("user_name"),
                    commentText = reader.GetString("comment_text"),
                    createdAt = reader.GetDateTime("created_at")
                });
            return Ok(new { comments, success = true });
        }

    }
}
namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("news/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public NotificationsController(IConfiguration config)
        {
            _config = config;
        }

        private MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            return conn;
        }
        [HttpGet("")]
        public IActionResult GetNotifications()
        {
            using var conn = OpenConnection();
            var cmd = new MySqlCommand(
                "SELECT id, news_article_id, user_id, notification_type, user_name, action_text, is_read, created_at FROM admin_notifications ORDER BY created_at DESC LIMIT 50", conn);
            var notifications = new List<object>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                notifications.Add(new
                {
                    id = reader.GetInt32("id"),
                    articleId = reader.GetInt32("news_article_id"),
                    userId = reader.IsDBNull(reader.GetOrdinal("user_id")) ? (int?)null : reader.GetInt32("user_id"),
                    type = reader.GetString("notification_type"),
                    userName = reader.GetString("user_name"),
                    actionText = reader.GetString("action_text"),
                    isRead = reader.GetBoolean("is_read"),
                    createdAt = reader.GetDateTime("created_at")
                });
            return Ok(new { notifications, success = true });
        }
        [HttpGet("unread-count")]
        public IActionResult GetUnreadCount()
        {
            using var conn = OpenConnection();
            var cmd = new MySqlCommand("SELECT COUNT(*) FROM admin_notifications WHERE is_read = FALSE", conn);
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return Ok(new { unreadCount = count, success = true });
        }
        [HttpPost("{id:int}/read")]
        public IActionResult MarkRead(int id)
        {
            using var conn = OpenConnection();
            var cmd = new MySqlCommand("UPDATE admin_notifications SET is_read = TRUE WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            int rowsRead = cmd.ExecuteNonQuery();
            if (rowsRead == 0) return NotFound(new { message = "Notification not found.", success = false });
            return Ok(new { message = "Notification marked as read.", success = true });
        }
        [HttpDelete("{id:int}")]
        public IActionResult DeleteNotification(int id)
        {
            using var conn = OpenConnection();
            var cmd = new MySqlCommand("DELETE FROM admin_notifications WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            int rowsDeleted = cmd.ExecuteNonQuery();
            if (rowsDeleted == 0) return NotFound(new { message = "Notification not found.", success = false });
            return Ok(new { message = "Notification deleted.", success = true });
        }
    }
}