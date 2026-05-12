using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("culture")]
    public class CultureController : ControllerBase
    {
        private readonly IConfiguration _config;

        private static readonly string[] ValidStatuses =
            { "draft", "published", "archived" };

        public CultureController(IConfiguration config)
        {
            _config = config;
        }

        // ─── PUBLIC ENDPOINTS ─────────────────────────────────────

        /// <summary>GET /culture/articles — Public, published only</summary>
        [HttpGet("articles")]
        public IActionResult GetArticles()
        {
            using var conn = OpenConnection();
            return Ok(QueryArticles(conn, "status = 'published'",
                orderBy: "is_pinned DESC, pin_order ASC, published_at DESC"));
        }

        /// <summary>GET /culture/articles/{id} — Public, published only</summary>
        [HttpGet("articles/{id:int}")]
        public IActionResult GetArticle(int id)
        {
            using var conn = OpenConnection();
            var article = GetArticleById(conn, id);
            if (article == null || article.Status != "published")
                return NotFound(new { message = "Article not found." });
            return Ok(article);
        }

        // ─── ADMIN ENDPOINTS ──────────────────────────────────────

        /// <summary>GET /culture/admin/articles — Admin, all statuses</summary>
        [Authorize(Roles = "admin")]
        [HttpGet("admin/articles")]
        public IActionResult AdminGetArticles([FromQuery] string? status = null)
        {
            using var conn = OpenConnection();
            string where = string.IsNullOrWhiteSpace(status) ? "1=1" : "status = @Status";
            var cmd = new MySqlCommand($@"
                SELECT id, title, excerpt, author, read_time_minutes,
                       tone, is_pinned, pin_order, status, published_at, created_at, updated_at
                FROM culture_articles
                WHERE {where}
                ORDER BY is_pinned DESC, pin_order ASC, updated_at DESC", conn);
            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@Status", status);
            var list = new List<CultureStory>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapArticle(reader));
            return Ok(list);
        }

        /// <summary>GET /culture/admin/articles/{id} — Admin, any status</summary>
        [Authorize(Roles = "admin")]
        [HttpGet("admin/articles/{id:int}")]
        public IActionResult AdminGetArticle(int id)
        {
            using var conn = OpenConnection();
            var article = GetArticleById(conn, id);
            if (article == null) return NotFound(new { message = "Article not found." });
            return Ok(article);
        }

        /// <summary>POST /culture/admin/articles — Admin</summary>
        [Authorize(Roles = "admin")]
        [HttpPost("admin/articles")]
        public IActionResult CreateArticle([FromBody] CreateCultureStoryRequest req)
        {
            if (!ValidStatuses.Contains(req.Status))
                return BadRequest(new { message = $"Invalid status. Must be one of: {string.Join(", ", ValidStatuses)}" });

            using var conn = OpenConnection();
            var now = DateTime.UtcNow;
            var cmd = new MySqlCommand(@"
                INSERT INTO culture_articles
                    (title, excerpt, author, read_time_minutes, tone,
                     is_pinned, pin_order, status, published_at, created_at, updated_at)
                VALUES
                    (@Title, @Excerpt, @Author, @ReadTime, @Tone,
                     @IsPinned, @PinOrder, @Status, @PublishedAt, @Now, @Now);
                SELECT LAST_INSERT_ID();", conn);
            BindParams(cmd, req, now);
            var newId = Convert.ToInt32(cmd.ExecuteScalar());
            return CreatedAtAction(nameof(AdminGetArticle), new { id = newId }, GetArticleById(conn, newId));
        }

        /// <summary>PUT /culture/admin/articles/{id} — Admin</summary>
        [Authorize(Roles = "admin")]
        [HttpPut("admin/articles/{id:int}")]
        public IActionResult UpdateArticle(int id, [FromBody] UpdateCultureStoryRequest req)
        {
            if (!ValidStatuses.Contains(req.Status))
                return BadRequest(new { message = $"Invalid status. Must be one of: {string.Join(", ", ValidStatuses)}" });

            using var conn = OpenConnection();
            if (GetArticleById(conn, id) == null)
                return NotFound(new { message = "Article not found." });

            var now = DateTime.UtcNow;
            var cmd = new MySqlCommand(@"
                UPDATE culture_articles SET
                    title             = @Title,
                    excerpt           = @Excerpt,
                    author            = @Author,
                    read_time_minutes = @ReadTime,
                    tone              = @Tone,
                    is_pinned         = @IsPinned,
                    pin_order         = @PinOrder,
                    status            = @Status,
                    published_at      = @PublishedAt,
                    updated_at        = @Now
                WHERE id = @Id", conn);
            BindParams(cmd, req, now);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
            return Ok(GetArticleById(conn, id));
        }

        /// <summary>PATCH /culture/admin/articles/{id}/status — Admin</summary>
        [Authorize(Roles = "admin")]
        [HttpPatch("admin/articles/{id:int}/status")]
        public IActionResult PatchStatus(int id, [FromBody] PatchStatusRequest req)
        {
            if (!ValidStatuses.Contains(req.Status))
                return BadRequest(new { message = $"Invalid status. Must be one of: {string.Join(", ", ValidStatuses)}" });

            using var conn = OpenConnection();
            var existing = GetArticleById(conn, id);
            if (existing == null) return NotFound(new { message = "Article not found." });

            var now = DateTime.UtcNow;
            var pubAt = req.Status == "published" ? (existing.PublishedAt ?? now) : existing.PublishedAt;

            var cmd = new MySqlCommand(@"
                UPDATE culture_articles
                SET status = @Status, published_at = @PublishedAt, updated_at = @Now
                WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@Status", req.Status);
            cmd.Parameters.AddWithValue("@PublishedAt", (object?)pubAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Now", now);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
            return Ok(GetArticleById(conn, id));
        }

        /// <summary>PATCH /culture/admin/articles/{id}/pin — Admin</summary>
        [Authorize(Roles = "admin")]
        [HttpPatch("admin/articles/{id:int}/pin")]
        public IActionResult PatchPin(int id, [FromBody] PatchPinRequest req)
        {
            using var conn = OpenConnection();
            if (GetArticleById(conn, id) == null)
                return NotFound(new { message = "Article not found." });

            var cmd = new MySqlCommand(@"
                UPDATE culture_articles
                SET is_pinned = @IsPinned, pin_order = @PinOrder, updated_at = @Now
                WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@IsPinned", req.IsPinned);
            cmd.Parameters.AddWithValue("@PinOrder", req.PinOrder);
            cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
            return Ok(GetArticleById(conn, id));
        }

        /// <summary>DELETE /culture/admin/articles/{id} — Admin</summary>
        [Authorize(Roles = "admin")]
        [HttpDelete("admin/articles/{id:int}")]
        public IActionResult DeleteArticle(int id)
        {
            using var conn = OpenConnection();
            if (GetArticleById(conn, id) == null)
                return NotFound(new { message = "Article not found." });

            var cmd = new MySqlCommand("DELETE FROM culture_articles WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Article deleted." });
        }

        // ─── HELPERS ─────────────────────────────────────────────

        private MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            return conn;
        }

        private static List<CultureStory> QueryArticles(
            MySqlConnection conn, string where,
            string orderBy = "published_at DESC")
        {
            var cmd = new MySqlCommand($@"
                SELECT id, title, excerpt, author, read_time_minutes,
                       tone, is_pinned, pin_order, status, published_at, created_at, updated_at
                FROM culture_articles
                WHERE {where}
                ORDER BY {orderBy}", conn);
            var list = new List<CultureStory>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapArticle(reader));
            return list;
        }

        private static CultureStory? GetArticleById(MySqlConnection conn, int id)
        {
            var cmd = new MySqlCommand(@"
                SELECT id, title, excerpt, author, read_time_minutes,
                       tone, is_pinned, pin_order, status, published_at, created_at, updated_at
                FROM culture_articles WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapArticle(reader) : null;
        }

        private static void BindParams(MySqlCommand cmd, CreateCultureStoryRequest req, DateTime now)
        {
            cmd.Parameters.AddWithValue("@Title", req.Title);
            cmd.Parameters.AddWithValue("@Excerpt", req.Excerpt);
            cmd.Parameters.AddWithValue("@Author", req.Author);
            cmd.Parameters.AddWithValue("@ReadTime", req.ReadTimeMinutes);
            cmd.Parameters.AddWithValue("@Tone", req.Tone);
            cmd.Parameters.AddWithValue("@IsPinned", req.IsPinned);
            cmd.Parameters.AddWithValue("@PinOrder", req.PinOrder);
            cmd.Parameters.AddWithValue("@Status", req.Status);
            cmd.Parameters.AddWithValue("@PublishedAt", (object?)req.PublishedAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Now", now);
        }

        private static CultureStory MapArticle(MySqlDataReader r) => new CultureStory
        {
            Id = r.GetInt32("id"),
            Title = r.GetString("title"),
            Excerpt = r.IsDBNull(r.GetOrdinal("excerpt")) ? "" : r.GetString("excerpt"),
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
    }
}