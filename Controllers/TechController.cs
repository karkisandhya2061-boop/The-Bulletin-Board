using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("tech")]
    public class TechController : ControllerBase
    {
        private readonly IConfiguration _config;
        private const string Category = "tech";

        public TechController(IConfiguration config)
        {
            _config = config;
        }
        [HttpGet("feed")]
        public IActionResult GetTechFeed()
        {
            using var conn = OpenConnection();

            var allStories = QueryTechStories(conn,
                where: "status = 'published' AND category = @Category",
                parameters: new Dictionary<string, object?> { ["@Category"] = Category },
                orderBy: "published_at DESC, created_at DESC");

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

            return Ok(new TechFeedResponse
            {
                HeroStory = hero,
                FeaturedSideStories = featured,
                TrendingStories = trending
            });
        }
        [HttpGet("stories")]
        public IActionResult GetTechStories([FromQuery] string? bucket = null)
        {
            using var conn = OpenConnection();

            var conditions = new List<string>
            {
                "status = 'published'",
                "category = @Category"
            };
            var parameters = new Dictionary<string, object?> { ["@Category"] = Category };

            if (!string.IsNullOrWhiteSpace(bucket))
            {
                conditions.Add("bucket = @Bucket");
                parameters["@Bucket"] = bucket;
            }

            string where = string.Join(" AND ", conditions);
            string orderBy = bucket == "trendingStories"
                ? "is_pinned DESC, pin_order ASC, updated_at DESC"
                : "published_at DESC, created_at DESC";

            return Ok(QueryTechStories(conn, where, parameters, orderBy));
        }
        [HttpGet("stories/{id:int}")]
        public IActionResult GetTechStory(int id)
        {
            using var conn = OpenConnection();
            var story = GetById(conn, id);

            if (story == null || story.Status != "published")
                return NotFound(new { message = "Tech story not found." });

            return Ok(story);
        }
        [Authorize(Roles = "admin")]
        [HttpPost("admin/stories")]
        public IActionResult CreateTechStory([FromBody] CreateStoryRequest req)
        {
            var validBuckets = new[] { "heroStory", "featuredSideStories", "trendingStories" };
            var validStatuses = new[] { "draft", "published", "archived" };

            if (!validBuckets.Contains(req.Bucket))
                return BadRequest(new { message = $"Invalid bucket. Must be: {string.Join(", ", validBuckets)}" });
            if (!validStatuses.Contains(req.Status))
                return BadRequest(new { message = $"Invalid status. Must be: {string.Join(", ", validStatuses)}" });

            // Force category = "tech"
            req.Category = Category;

            using var conn = OpenConnection();
            var now = DateTime.UtcNow;

            var cmd = new MySqlCommand(@"
                INSERT INTO stories
                    (bucket, category, title, excerpt, author, read_time_minutes,
                     tone, is_pinned, pin_order, status, published_at, created_at, updated_at)
                VALUES
                    (@Bucket, @Category, @Title, @Excerpt, @Author, @ReadTime,
                     @Tone, @IsPinned, @PinOrder, @Status, @PublishedAt, @Now, @Now);
                SELECT LAST_INSERT_ID();", conn);

            BindParams(cmd, req, now);
            var newId = Convert.ToInt32(cmd.ExecuteScalar());

            return CreatedAtAction(nameof(GetTechStory), new { id = newId }, GetById(conn, newId));
        }
        [Authorize(Roles = "admin")]
        [HttpPut("admin/stories/{id:int}")]
        public IActionResult UpdateTechStory(int id, [FromBody] CreateStoryRequest req)
        {
            req.Category = Category;

            using var conn = OpenConnection();
            if (GetById(conn, id) == null)
                return NotFound(new { message = "Tech story not found." });

            var now = DateTime.UtcNow;
            var cmd = new MySqlCommand(@"
                UPDATE stories SET
                    bucket            = @Bucket,
                    category          = @Category,
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
                WHERE id = @Id AND category = 'tech'", conn);

            BindParams(cmd, req, now);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();

            return Ok(GetById(conn, id));
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("admin/stories/{id:int}")]
        public IActionResult DeleteTechStory(int id)
        {
            using var conn = OpenConnection();
            if (GetById(conn, id) == null)
                return NotFound(new { message = "Tech story not found." });

            var cmd = new MySqlCommand("DELETE FROM stories WHERE id = @Id AND category = 'tech'", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();

            return Ok(new { message = "Tech story deleted." });
        }

        private MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            return conn;
        }

        private static List<TechStory> QueryTechStories(
            MySqlConnection conn,
            string where,
            Dictionary<string, object?> parameters,
            string orderBy)
        {
            var cmd = new MySqlCommand($@"
                SELECT id, bucket, title, excerpt, author,
                       read_time_minutes, tone, is_pinned, pin_order,
                       status, published_at, created_at, updated_at
                FROM stories
                WHERE {where}
                ORDER BY {orderBy}", conn);

            foreach (var (k, v) in parameters)
                cmd.Parameters.AddWithValue(k, v ?? DBNull.Value);

            var list = new List<TechStory>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapRow(reader));
            return list;
        }

        private static TechStory? GetById(MySqlConnection conn, int id)
        {
            var cmd = new MySqlCommand(@"
                SELECT id, bucket, title, excerpt, author,
                       read_time_minutes, tone, is_pinned, pin_order,
                       status, published_at, created_at, updated_at
                FROM stories
                WHERE id = @Id AND category = 'tech'", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapRow(reader) : null;
        }

        private static void BindParams(MySqlCommand cmd, CreateStoryRequest req, DateTime now)
        {
            cmd.Parameters.AddWithValue("@Bucket", req.Bucket);
            cmd.Parameters.AddWithValue("@Category", req.Category);
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

        private static TechStory MapRow(MySqlDataReader r) => new TechStory
        {
            Id = r.GetInt32("id"),
            Bucket = r.GetString("bucket"),
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