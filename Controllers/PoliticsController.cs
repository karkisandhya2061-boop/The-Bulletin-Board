using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("politics")]
    public class PoliticsController : ControllerBase
    {
        private readonly IConfiguration _config;

        private static readonly string[] ValidStatuses =
            { "draft", "published", "archived" };

        public PoliticsController(IConfiguration config)
        {
            _config = config;
        }
        // Return published politics articles with optional filters
        // Return a single published politics article
        [HttpGet("articles")]
        public IActionResult GetArticles(
            [FromQuery] bool? featured = null,
            [FromQuery] string? tag = null,
            [FromQuery] string? author = null)
        {
            using var conn = OpenConnection();

            var conditions = new List<string> { "status = 'published'" };
            var parameters = new Dictionary<string, object?>();

            if (featured.HasValue)
            {
                conditions.Add("is_featured = @IsFeatured");
                parameters["@IsFeatured"] = featured.Value ? 1 : 0;
            }
            if (!string.IsNullOrWhiteSpace(tag))
            {
                // FIND_IN_SET works for comma-separated tag storage
                conditions.Add("FIND_IN_SET(@Tag, tags) > 0");
                parameters["@Tag"] = tag.Trim();
            }
            if (!string.IsNullOrWhiteSpace(author))
            {
                conditions.Add("author = @Author");
                parameters["@Author"] = author.Trim();
            }

            string where = string.Join(" AND ", conditions);

            return Ok(QueryArticles(conn, where, parameters,
                orderBy: "is_featured DESC, is_pinned DESC, pin_order ASC, published_at DESC, created_at DESC"));
        }
        [HttpGet("articles/{id:int}")]
        public IActionResult GetArticle(int id)
        {
            using var conn = OpenConnection();
            var article = GetArticleById(conn, id);

            if (article == null || article.Status != "published")
                return NotFound(new { message = "Article not found." });

            return Ok(article);
        }
        // Return the latest featured politics article
        [HttpGet("featured")]
        public IActionResult GetFeatured()
        {
            using var conn = OpenConnection();
            var articles = QueryArticles(conn,
                where: "status = 'published' AND is_featured = 1",
                parameters: null,
                orderBy: "published_at DESC, created_at DESC");

            var top = articles.FirstOrDefault();
            if (top == null)
                return NotFound(new { message = "No featured politics article found." });

            return Ok(top);
        }
        // Admin: get all politics articles
        [Authorize(Roles = "admin")]
        [HttpGet("admin/articles")]
        public IActionResult AdminGetArticles(
            [FromQuery] string? status = null,
            [FromQuery] bool? featured = null,
            [FromQuery] string? tag = null,
            [FromQuery] string? author = null)
        {
            using var conn = OpenConnection();

            var conditions = new List<string>();
            var parameters = new Dictionary<string, object?>();

            if (!string.IsNullOrWhiteSpace(status))
            {
                conditions.Add("status = @Status");
                parameters["@Status"] = status;
            }
            if (featured.HasValue)
            {
                conditions.Add("is_featured = @IsFeatured");
                parameters["@IsFeatured"] = featured.Value ? 1 : 0;
            }
            if (!string.IsNullOrWhiteSpace(tag))
            {
                conditions.Add("FIND_IN_SET(@Tag, tags) > 0");
                parameters["@Tag"] = tag.Trim();
            }
            if (!string.IsNullOrWhiteSpace(author))
            {
                conditions.Add("author = @Author");
                parameters["@Author"] = author.Trim();
            }

            string where = conditions.Count > 0
                ? string.Join(" AND ", conditions)
                : "1=1";

            return Ok(QueryArticles(conn, where, parameters,
                orderBy: "is_featured DESC, is_pinned DESC, pin_order ASC, updated_at DESC"));
        }
        [Authorize(Roles = "admin")]
        [HttpGet("admin/articles/{id:int}")]
        public IActionResult AdminGetArticle(int id)
        {
            using var conn = OpenConnection();
            var article = GetArticleById(conn, id);
            if (article == null) return NotFound(new { message = "Article not found." });
            return Ok(article);
        }
        [Authorize(Roles = "admin")]
        [HttpPost("admin/articles")]
        public IActionResult CreateArticle([FromBody] CreatePoliticsArticleRequest req)
        {
            if (!ValidStatuses.Contains(req.Status))
                return BadRequest(new { message = $"Invalid status. Must be one of: {string.Join(", ", ValidStatuses)}" });

            using var conn = OpenConnection();

            // Only one featured article published at a time — demote any existing one
            if (req.IsFeatured && req.Status == "published")
                DemoteExistingFeatured(conn);

            var now = DateTime.UtcNow;
            var cmd = new MySqlCommand(@"
                INSERT INTO politics_articles
                    (title, excerpt, content, author, image_url, tags,
                     read_time_minutes, is_featured, is_pinned, pin_order,
                     status, published_at, created_at, updated_at)
                VALUES
                    (@Title, @Excerpt, @Content, @Author, @ImageUrl, @Tags,
                     @ReadTime, @IsFeatured, @IsPinned, @PinOrder,
                     @Status, @PublishedAt, @Now, @Now);
                SELECT LAST_INSERT_ID();", conn);

            BindArticleParams(cmd, req, now);
            var newId = Convert.ToInt32(cmd.ExecuteScalar());
            return CreatedAtAction(nameof(AdminGetArticle), new { id = newId }, GetArticleById(conn, newId));
        }
        [Authorize(Roles = "admin")]
        [HttpPut("admin/articles/{id:int}")]
        public IActionResult UpdateArticle(int id, [FromBody] UpdatePoliticsArticleRequest req)
        {
            if (!ValidStatuses.Contains(req.Status))
                return BadRequest(new { message = $"Invalid status. Must be one of: {string.Join(", ", ValidStatuses)}" });

            using var conn = OpenConnection();
            if (GetArticleById(conn, id) == null)
                return NotFound(new { message = "Article not found." });

            if (req.IsFeatured && req.Status == "published")
                DemoteExistingFeatured(conn, excludeId: id);

            var now = DateTime.UtcNow;
            var cmd = new MySqlCommand(@"
                UPDATE politics_articles SET
                    title             = @Title,
                    excerpt           = @Excerpt,
                    content           = @Content,
                    author            = @Author,
                    image_url         = @ImageUrl,
                    tags              = @Tags,
                    read_time_minutes = @ReadTime,
                    is_featured       = @IsFeatured,
                    is_pinned         = @IsPinned,
                    pin_order         = @PinOrder,
                    status            = @Status,
                    published_at      = @PublishedAt,
                    updated_at        = @Now
                WHERE id = @Id", conn);

            BindArticleParams(cmd, req, now);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();

            return Ok(GetArticleById(conn, id));
        }
        [Authorize(Roles = "admin")]
        [HttpPatch("admin/articles/{id:int}/status")]
        public IActionResult PatchArticleStatus(int id, [FromBody] PatchPoliticsStatusRequest req)
        {
            if (!ValidStatuses.Contains(req.Status))
                return BadRequest(new { message = $"Invalid status. Must be one of: {string.Join(", ", ValidStatuses)}" });

            using var conn = OpenConnection();
            var existing = GetArticleById(conn, id);
            if (existing == null)
                return NotFound(new { message = "Article not found." });

            if (existing.IsFeatured && req.Status == "published")
                DemoteExistingFeatured(conn, excludeId: id);

            var now = DateTime.UtcNow;
            var pubAt = req.Status == "published"
                ? (existing.PublishedAt ?? now)
                : existing.PublishedAt;

            var cmd = new MySqlCommand(@"
                UPDATE politics_articles
                SET status = @Status, published_at = @PublishedAt, updated_at = @Now
                WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@Status", req.Status);
            cmd.Parameters.AddWithValue("@PublishedAt", (object?)pubAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Now", now);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();

            return Ok(GetArticleById(conn, id));
        }
        [Authorize(Roles = "admin")]
        [HttpPatch("admin/articles/{id:int}/pin")]
        public IActionResult PatchArticlePin(int id, [FromBody] PatchPoliticsPinRequest req)
        {
            using var conn = OpenConnection();
            if (GetArticleById(conn, id) == null)
                return NotFound(new { message = "Article not found." });

            var cmd = new MySqlCommand(@"
                UPDATE politics_articles
                SET is_pinned = @IsPinned, pin_order = @PinOrder, updated_at = @Now
                WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@IsPinned", req.IsPinned);
            cmd.Parameters.AddWithValue("@PinOrder", req.PinOrder);
            cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();

            return Ok(GetArticleById(conn, id));
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("admin/articles/{id:int}")]
        public IActionResult DeleteArticle(int id)
        {
            using var conn = OpenConnection();
            if (GetArticleById(conn, id) == null)
                return NotFound(new { message = "Article not found." });

            var cmd = new MySqlCommand("DELETE FROM politics_articles WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Article deleted." });
        }

        private MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            return conn;
        }

        private static List<PoliticsArticle> QueryArticles(
            MySqlConnection conn,
            string where,
            Dictionary<string, object?>? parameters,
            string orderBy = "published_at DESC, created_at DESC")
        {
            var cmd = new MySqlCommand($@"
                SELECT id, title, excerpt, content, author, image_url, tags,
                       read_time_minutes, is_featured, is_pinned, pin_order,
                       status, published_at, created_at, updated_at
                FROM politics_articles
                WHERE {where}
                ORDER BY {orderBy}", conn);

            if (parameters != null)
                foreach (var (k, v) in parameters)
                    cmd.Parameters.AddWithValue(k, v ?? DBNull.Value);

            var list = new List<PoliticsArticle>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapArticle(reader));
            return list;
        }

        private static PoliticsArticle? GetArticleById(MySqlConnection conn, int id)
        {
            var cmd = new MySqlCommand(@"
                SELECT id, title, excerpt, content, author, image_url, tags,
                       read_time_minutes, is_featured, is_pinned, pin_order,
                       status, published_at, created_at, updated_at
                FROM politics_articles WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapArticle(reader) : null;
        }

        private static void DemoteExistingFeatured(MySqlConnection conn, int excludeId = 0)
        {
            // Sets is_featured = 0 on the current featured published article
            var cmd = new MySqlCommand(@"
                UPDATE politics_articles
                SET is_featured = 0, updated_at = @Now
                WHERE is_featured = 1 AND status = 'published' AND id != @ExcludeId", conn);
            cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@ExcludeId", excludeId);
            cmd.ExecuteNonQuery();
        }

        private static void BindArticleParams(MySqlCommand cmd, CreatePoliticsArticleRequest req, DateTime now)
        {
            cmd.Parameters.AddWithValue("@Title", req.Title);
            cmd.Parameters.AddWithValue("@Excerpt", req.Excerpt);
            cmd.Parameters.AddWithValue("@Content", req.Content);
            cmd.Parameters.AddWithValue("@Author", req.Author);
            cmd.Parameters.AddWithValue("@ImageUrl", req.ImageUrl);
            cmd.Parameters.AddWithValue("@Tags", req.Tags);
            cmd.Parameters.AddWithValue("@ReadTime", req.ReadTimeMinutes);
            cmd.Parameters.AddWithValue("@IsFeatured", req.IsFeatured);
            cmd.Parameters.AddWithValue("@IsPinned", req.IsPinned);
            cmd.Parameters.AddWithValue("@PinOrder", req.PinOrder);
            cmd.Parameters.AddWithValue("@Status", req.Status);
            cmd.Parameters.AddWithValue("@PublishedAt", (object?)req.PublishedAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Now", now);
        }

        private static PoliticsArticle MapArticle(MySqlDataReader r) => new PoliticsArticle
        {
            Id = r.GetInt32("id"),
            Title = r.GetString("title"),
            Excerpt = r.IsDBNull(r.GetOrdinal("excerpt")) ? "" : r.GetString("excerpt"),
            Content = r.IsDBNull(r.GetOrdinal("content")) ? "" : r.GetString("content"),
            Author = r.IsDBNull(r.GetOrdinal("author")) ? "" : r.GetString("author"),
            ImageUrl = r.IsDBNull(r.GetOrdinal("image_url")) ? "" : r.GetString("image_url"),
            Tags = r.IsDBNull(r.GetOrdinal("tags")) ? "" : r.GetString("tags"),
            ReadTimeMinutes = r.GetInt32("read_time_minutes"),
            IsFeatured = r.GetBoolean("is_featured"),
            IsPinned = r.GetBoolean("is_pinned"),
            PinOrder = r.GetInt32("pin_order"),
            Status = r.GetString("status"),
            PublishedAt = r.IsDBNull(r.GetOrdinal("published_at")) ? null : r.GetDateTime("published_at"),
            CreatedAt = r.GetDateTime("created_at"),
            UpdatedAt = r.GetDateTime("updated_at"),
        };
    }
    public record PatchPoliticsStatusRequest(string Status);
    public record PatchPoliticsPinRequest(bool IsPinned, int PinOrder);
}