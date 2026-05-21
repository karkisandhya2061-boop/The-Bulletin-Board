using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("news")]
    public class NewsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public NewsController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("debug/latest")]
        public IActionResult GetLatestNewsDebug()
        {
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new MySqlConnection(connStr);
                conn.Open();

                string query = @"
                    SELECT 
                        id, 
                        title, 
                        excerpt, 
                        location,
                        created_at,
                        LENGTH(image_url) as image_size,
                        IF(image_url IS NULL OR image_url = '', 'NO IMAGE', 'HAS IMAGE') as image_status
                    FROM news_articles
                    ORDER BY created_at DESC
                    LIMIT 10";

                var cmd = new MySqlCommand(query, conn);
                var articles = new List<object>();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    articles.Add(new
                    {
                        id = reader.GetInt32("id"),
                        title = reader.GetString("title"),
                        excerpt = reader.IsDBNull(reader.GetOrdinal("excerpt")) ? "" : reader.GetString("excerpt"),
                        location = reader.IsDBNull(reader.GetOrdinal("location")) ? "" : reader.GetString("location"),
                        created_at = reader.GetDateTime("created_at"),
                        image_size = reader.GetInt32("image_size"),
                        image_status = reader.GetString("image_status")
                    });
                }

                return Ok(new { articles, message = "Debug info for latest articles" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetNewsById(int id)
        {
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new MySqlConnection(connStr);
                conn.Open();

                string query = @"
                    SELECT id, title, content, excerpt, image_url, location, created_at
                    FROM news_articles
                    WHERE id = @id";

                var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return Ok(new
                    {
                        id = reader.GetInt32("id"),
                        title = reader.GetString("title"),
                        excerpt = reader.IsDBNull(reader.GetOrdinal("excerpt")) ? "" : reader.GetString("excerpt"),
                        content = reader.GetString("content"),
                        location = reader.IsDBNull(reader.GetOrdinal("location")) ? "" : reader.GetString("location"),
                        imageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? "" : reader.GetString("image_url"),
                        created_at = reader.GetDateTime("created_at")
                    });
                }

                return NotFound(new { message = "Article not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("test-feed")]
        public IActionResult TestFeed()
        {
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new MySqlConnection(connStr);
                conn.Open();

                string query = @"
                    SELECT id, title, image_url
                    FROM news_articles
                    WHERE status = 'published' AND image_url IS NOT NULL AND image_url != ''
                    ORDER BY created_at DESC
                    LIMIT 5";

                var cmd = new MySqlCommand(query, conn);
                var articles = new List<object>();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string imageUrl = reader.GetString("image_url");
                    articles.Add(new
                    {
                        id = reader.GetInt32("id"),
                        title = reader.GetString("title"),
                        imageSize = imageUrl.Length,
                        imagePreview = imageUrl.Length > 50 ? imageUrl.Substring(0, 50) + "..." : imageUrl,
                        hasImage = !string.IsNullOrEmpty(imageUrl)
                    });
                }

                return Ok(new { articles, totalCount = articles.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("feed")]
        public IActionResult GetFeed()
        {
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new MySqlConnection(connStr);
                conn.Open();

                string query = @"
                    SELECT id, title, content, excerpt, category_id, status, published_at, created_at, image_url, location
                    FROM news_articles
                    WHERE status = 'published'
                    ORDER BY published_at DESC
                    LIMIT 20";

                var cmd = new MySqlCommand(query, conn);
                var articles = new List<object>();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    articles.Add(new
                    {
                        id = reader.GetInt32("id"),
                        title = reader.GetString("title"),
                        excerpt = reader.IsDBNull(reader.GetOrdinal("excerpt")) ? "" : reader.GetString("excerpt"),
                        category = "General",
                        author = "Editorial Team",
                        time = "5 min read",
                        tone = new[] { "blue", "amber", "teal", "rose" }[new Random().Next(4)],
                        tag = "New",
                        imageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? "" : reader.GetString("image_url"),
                        location = reader.IsDBNull(reader.GetOrdinal("location")) ? "General" : reader.GetString("location")
                    });
                }

                return Ok(new
                {
                    heroStory = articles.Count > 0 ? articles[0] : null,
                    featuredSideStories = articles.Count > 1 ? articles.Skip(1).Take(2).ToList() : new List<object>(),
                    trendingStories = articles.Count > 0 ? articles.Take(Math.Min(4, articles.Count)).ToList() : new List<object>(),
                    tickerItems = new[] { 
                        "Breaking: New article published", 
                        "Latest news updates available", 
                        "Stay informed with our news portal" 
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("category/{category}")]
        public IActionResult GetByCategory(string category)
        {
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new MySqlConnection(connStr);
                conn.Open();

                string query = @"
                    SELECT id, title, content, excerpt, category_id, status, published_at, image_url, location
                    FROM news_articles
                    WHERE status = 'published' 
                    AND (category_id = (SELECT id FROM categories WHERE name = @category) OR @category = 'all')
                    ORDER BY published_at DESC
                    LIMIT 20";

                var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@category", category);
                var articles = new List<object>();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    articles.Add(new
                    {
                        id = reader.GetInt32("id"),
                        title = reader.GetString("title"),
                        excerpt = reader.IsDBNull(reader.GetOrdinal("excerpt")) ? "" : reader.GetString("excerpt"),
                        category = category,
                        imageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? "" : reader.GetString("image_url"),
                        location = reader.IsDBNull(reader.GetOrdinal("location")) ? "General" : reader.GetString("location"),
                        author = "Editorial Team",
                        time = "5 min read",
                        tone = new[] { "blue", "amber", "teal", "rose" }[new Random().Next(4)],
                        tag = "New"
                    });
                }

                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("search")]
        public IActionResult SearchNews([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    return BadRequest(new { message = "Search query is required", results = new List<object>() });
                }

                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new MySqlConnection(connStr);
                conn.Open();

                string query = @"
                    SELECT id, title, content, excerpt, category_id, status, published_at, image_url, location
                    FROM news_articles
                    WHERE status = 'published' 
                    AND (title LIKE @searchTerm OR excerpt LIKE @searchTerm OR content LIKE @searchTerm OR location LIKE @searchTerm)
                    ORDER BY published_at DESC
                    LIMIT 20";

                var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@searchTerm", $"%{q}%");
                var articles = new List<object>();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    articles.Add(new
                    {
                        id = reader.GetInt32("id"),
                        title = reader.GetString("title"),
                        excerpt = reader.IsDBNull(reader.GetOrdinal("excerpt")) ? "" : reader.GetString("excerpt"),
                        category = "General",
                        imageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? "" : reader.GetString("image_url"),
                        location = reader.IsDBNull(reader.GetOrdinal("location")) ? "General" : reader.GetString("location"),
                        author = "Editorial Team",
                        time = "5 min read",
                        tone = new[] { "blue", "amber", "teal", "rose" }[new Random().Next(4)],
                        tag = "Search Result"
                    });
                }

                return Ok(new { results = articles, query = q, count = articles.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("create")]
        public IActionResult CreateNews([FromBody] CreateNewsRequest request)
        {
            try
            {
                // Validate input
                if (request == null)
                {
                    return BadRequest(new { message = "Request body is required", success = false });
                }

                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return BadRequest(new { message = "Title is required", success = false });
                }

                if (string.IsNullOrWhiteSpace(request.Content))
                {
                    return BadRequest(new { message = "Content is required", success = false });
                }

                if (string.IsNullOrWhiteSpace(request.ImageUrl))
                {
                    return BadRequest(new { message = "Image URL is required", success = false });
                }

                // Check image URL size
                if (request.ImageUrl.Length > 16777215) // Max for MEDIUMTEXT
                {
                    return BadRequest(new { message = $"Image is too large ({request.ImageUrl.Length} bytes). Max is 16MB.", success = false });
                }

                Console.WriteLine($"[NEWS] Creating article: {request.Title}");
                Console.WriteLine($"[NEWS] Image size: {request.ImageUrl.Length} bytes");

                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new MySqlConnection(connStr);
                conn.Open();

                // Get category ID
                int categoryId = 1; // Default to first category
                if (!string.IsNullOrEmpty(request.Category))
                {
                    string catQuery = "SELECT id FROM categories WHERE name = @category LIMIT 1";
                    var catCmd = new MySqlCommand(catQuery, conn);
                    catCmd.Parameters.AddWithValue("@category", request.Category);
                    var catResult = catCmd.ExecuteScalar();
                    if (catResult != null)
                    {
                        categoryId = Convert.ToInt32(catResult);
                    }
                }

                // Get or create author (admin user)
                int authorId = 1;
                string authorQuery = "SELECT id FROM users WHERE role = 'admin' LIMIT 1";
                var authorCmd = new MySqlCommand(authorQuery, conn);
                var authorResult = authorCmd.ExecuteScalar();
                if (authorResult != null)
                {
                    authorId = Convert.ToInt32(authorResult);
                }

                // Insert news article
                string insertQuery = @"
                    INSERT INTO news_articles (title, content, excerpt, category_id, author_id, status, published_at, created_at, updated_at, image_url, location)
                    VALUES (@title, @content, @excerpt, @categoryId, @authorId, 'published', NOW(), NOW(), NOW(), @imageUrl, @location)";

                var insertCmd = new MySqlCommand(insertQuery, conn);
                insertCmd.Parameters.AddWithValue("@title", request.Title);
                insertCmd.Parameters.AddWithValue("@content", request.Content);
                insertCmd.Parameters.AddWithValue("@excerpt", request.Excerpt ?? "");
                insertCmd.Parameters.AddWithValue("@categoryId", categoryId);
                insertCmd.Parameters.AddWithValue("@authorId", authorId);
                insertCmd.Parameters.AddWithValue("@imageUrl", request.ImageUrl ?? "");
                insertCmd.Parameters.AddWithValue("@location", request.Location ?? "General");

                int rowsAffected = insertCmd.ExecuteNonQuery();
                Console.WriteLine($"[NEWS] Insert result: {rowsAffected} rows affected");

                if (rowsAffected > 0)
                {
                    // Verify the insert by querying back
                    string verifyQuery = "SELECT image_url FROM news_articles WHERE title = @title ORDER BY created_at DESC LIMIT 1";
                    var verifyCmd = new MySqlCommand(verifyQuery, conn);
                    verifyCmd.Parameters.AddWithValue("@title", request.Title);
                    var verifyResult = verifyCmd.ExecuteScalar();
                    
                    if (verifyResult != null && verifyResult != DBNull.Value)
                    {
                        string savedImage = verifyResult.ToString();
                        Console.WriteLine($"[NEWS] Verification: Image saved, size={savedImage.Length} bytes");
                    }
                    else
                    {
                        Console.WriteLine("[NEWS] WARNING: Image not found in verification query!");
                    }
                }

                return Ok(new { message = "News article created successfully", success = true });
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[NEWS] MySql Error: {ex.Message}");
                return StatusCode(500, new { message = $"Database error: {ex.Message}", success = false });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NEWS] Error: {ex.Message}");
                return StatusCode(500, new { message = $"Error: {ex.Message}", success = false });
            }
        }

        [HttpPost("{id}/reactions")]
        public IActionResult AddReaction(int id, [FromBody] ReactionRequest request)
        {
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new MySqlConnection(connStr);
                conn.Open();

                // Check if article exists
                string checkQuery = "SELECT id FROM news_articles WHERE id = @id";
                var checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@id", id);
                var result = checkCmd.ExecuteScalar();
                
                if (result == null)
                {
                    return NotFound(new { message = "Article not found", success = false });
                }

                // Insert or update reaction
                string query = @"
                    INSERT INTO news_reactions (news_article_id, user_id, reaction_type)
                    VALUES (@newsId, @userId, @reactionType)
                    ON DUPLICATE KEY UPDATE reaction_type = @reactionType";

                var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@newsId", id);
                cmd.Parameters.AddWithValue("@userId", request.UserId ?? 0);
                cmd.Parameters.AddWithValue("@reactionType", request.ReactionType);

                cmd.ExecuteNonQuery();

                // Create notification for admin
                string notifQuery = @"
                    INSERT INTO admin_notifications (news_article_id, notification_type, user_name, action_text)
                    VALUES (@newsId, 'reaction', @userName, @actionText)";
                
                var notifCmd = new MySqlCommand(notifQuery, conn);
                notifCmd.Parameters.AddWithValue("@newsId", id);
                notifCmd.Parameters.AddWithValue("@userName", request.UserName ?? "User");
                notifCmd.Parameters.AddWithValue("@actionText", $"User reacted with {request.ReactionType}");
                notifCmd.ExecuteNonQuery();

                // Get updated reaction counts
                string countQuery = @"
                    SELECT reaction_type, COUNT(*) as count
                    FROM news_reactions
                    WHERE news_article_id = @newsId
                    GROUP BY reaction_type";

                var countCmd = new MySqlCommand(countQuery, conn);
                countCmd.Parameters.AddWithValue("@newsId", id);
                var reactions = new Dictionary<string, int>();

                using var reader = countCmd.ExecuteReader();
                while (reader.Read())
                {
                    reactions[reader.GetString("reaction_type")] = reader.GetInt32("count");
                }

                return Ok(new { message = "Reaction added", success = true, reactions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, success = false });
            }
        }

        [HttpGet("{id}/reactions")]
        public IActionResult GetReactions(int id)
        {
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new MySqlConnection(connStr);
                conn.Open();

                string query = @"
                    SELECT reaction_type, COUNT(*) as count
                    FROM news_reactions
                    WHERE news_article_id = @newsId
                    GROUP BY reaction_type";

                var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@newsId", id);
                var reactions = new Dictionary<string, int>();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    reactions[reader.GetString("reaction_type")] = reader.GetInt32("count");
                }

                return Ok(new { reactions, success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{id}/comments")]
        public IActionResult AddComment(int id, [FromBody] CommentRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.CommentText))
                {
                    return BadRequest(new { message = "Comment text is required", success = false });
                }

                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new MySqlConnection(connStr);
                conn.Open();

                // Check if article exists
                string checkQuery = "SELECT id FROM news_articles WHERE id = @id";
                var checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@id", id);
                var result = checkCmd.ExecuteScalar();
                
                if (result == null)
                {
                    return NotFound(new { message = "Article not found", success = false });
                }

                // Insert comment
                string insertQuery = @"
                    INSERT INTO news_comments (news_article_id, user_id, user_name, comment_text)
                    VALUES (@newsId, @userId, @userName, @commentText)";

                var cmd = new MySqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@newsId", id);
                cmd.Parameters.AddWithValue("@userId", request.UserId ?? 0);
                cmd.Parameters.AddWithValue("@userName", request.UserName ?? "Anonymous");
                cmd.Parameters.AddWithValue("@commentText", request.CommentText);

                cmd.ExecuteNonQuery();

                // Create notification for admin
                string notifQuery = @"
                    INSERT INTO admin_notifications (news_article_id, notification_type, user_name, action_text)
                    VALUES (@newsId, 'comment', @userName, @actionText)";
                
                var notifCmd = new MySqlCommand(notifQuery, conn);
                notifCmd.Parameters.AddWithValue("@newsId", id);
                notifCmd.Parameters.AddWithValue("@userName", request.UserName ?? "Anonymous");
                notifCmd.Parameters.AddWithValue("@actionText", $"New comment: {request.CommentText.Substring(0, Math.Min(50, request.CommentText.Length))}");
                notifCmd.ExecuteNonQuery();

                return Ok(new { message = "Comment added", success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, success = false });
            }
        }

        [HttpGet("{id}/comments")]
        public IActionResult GetComments(int id)
        {
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new MySqlConnection(connStr);
                conn.Open();

                string query = @"
                    SELECT id, user_name, comment_text, created_at
                    FROM news_comments
                    WHERE news_article_id = @newsId
                    ORDER BY created_at DESC
                    LIMIT 50";

                var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@newsId", id);
                var comments = new List<object>();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    comments.Add(new
                    {
                        id = reader.GetInt32("id"),
                        user_name = reader.GetString("user_name"),
                        comment_text = reader.GetString("comment_text"),
                        created_at = reader.GetDateTime("created_at")
                    });
                }

                return Ok(new { comments, success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("notifications")]
        public IActionResult GetNotifications()
        {
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new MySqlConnection(connStr);
                conn.Open();

                string query = @"
                    SELECT id, news_article_id, notification_type, user_name, action_text, is_read, created_at
                    FROM admin_notifications
                    ORDER BY created_at DESC
                    LIMIT 50";

                var cmd = new MySqlCommand(query, conn);
                var notifications = new List<object>();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    notifications.Add(new
                    {
                        id = reader.GetInt32("id"),
                        articleId = reader.GetInt32("news_article_id"),
                        type = reader.GetString("notification_type"),
                        userName = reader.GetString("user_name"),
                        actionText = reader.GetString("action_text"),
                        isRead = reader.GetBoolean("is_read"),
                        createdAt = reader.GetDateTime("created_at")
                    });
                }

                return Ok(new { notifications, success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("notifications/{id}/read")]
        public IActionResult MarkNotificationRead(int id)
        {
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new MySqlConnection(connStr);
                conn.Open();

                string query = @"
                    UPDATE admin_notifications
                    SET is_read = TRUE
                    WHERE id = @id";

                var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();

                return Ok(new { message = "Notification marked as read", success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("notifications/unread-count")]
        public IActionResult GetUnreadNotificationCount()
        {
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new MySqlConnection(connStr);
                conn.Open();

                string query = "SELECT COUNT(*) as count FROM admin_notifications WHERE is_read = FALSE";
                var cmd = new MySqlCommand(query, conn);
                var count = (int)cmd.ExecuteScalar();

                return Ok(new { unreadCount = count, success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }

    public class CreateNewsRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Excerpt { get; set; }
        public string Category { get; set; }
        public string Location { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ReactionRequest
    {
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string ReactionType { get; set; } // like, love, haha, wow, sad, angry
    }

    public class CommentRequest
    {
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string CommentText { get; set; }
    }
}
