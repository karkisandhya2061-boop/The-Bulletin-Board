using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Admin content management — stories.
    /// All endpoints require a valid JWT with role = "admin".
    /// Base route: /api/v1/admin/content
    /// </summary>
    [ApiController]
    [Route("api/v1/admin/content")]
    [Authorize(Roles = "admin")]
    public class ContentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ContentController(IConfiguration config)
        {
            _config = config;
        }

        // ─── CREATE ───────────────────────────────────────────────
        /// <summary>
        /// POST /api/v1/admin/content/stories
        /// Creates a new story. If isHero=true the previous hero is demoted to draft.
        /// </summary>
        [HttpPost("stories")]
        public IActionResult CreateStory([FromBody] ContentStoryRequest req)
        {
            var validation = ValidateRequest(req);
            if (validation != null) return validation;

            using var conn = OpenConnection();

            if (req.IsHero && req.Status == "published")
                DemoteCurrentHero(conn);

            var now = DateTime.UtcNow;
            var cmd = new MySqlCommand(@"
                INSERT INTO content_stories
                    (title, category, summary, status, is_hero, is_trending, created_at, updated_at)
                VALUES
                    (@Title, @Category, @Summary, @Status, @IsHero, @IsTrending, @Now, @Now);
                SELECT LAST_INSERT_ID();", conn);

            BindParams(cmd, req, now);
            var newId = Convert.ToInt32(cmd.ExecuteScalar());

            return CreatedAtAction(nameof(GetStory),
                new { id = newId },
                FetchById(conn, newId));
        }

        // ─── READ (single — used by CreatedAtAction) ──────────────
        [HttpGet("stories/{id:int}")]
        public IActionResult GetStory(int id)
        {
            using var conn = OpenConnection();
            var story = FetchById(conn, id);
            return story == null
                ? NotFound(new { message = "Story not found." })
                : Ok(story);
        }

        // ─── UPDATE ───────────────────────────────────────────────
        /// <summary>
        /// PUT /api/v1/admin/content/stories/:id
        /// Full replacement update. If isHero=true and status=published the old hero is demoted.
        /// </summary>
        [HttpPut("stories/{id:int}")]
        public IActionResult UpdateStory(int id, [FromBody] ContentStoryRequest req)
        {
            var validation = ValidateRequest(req);
            if (validation != null) return validation;

            using var conn = OpenConnection();

            if (FetchById(conn, id) == null)
                return NotFound(new { message = "Story not found." });

            if (req.IsHero && req.Status == "published")
                DemoteCurrentHero(conn, excludeId: id);

            var now = DateTime.UtcNow;
            var cmd = new MySqlCommand(@"
                UPDATE content_stories SET
                    title      = @Title,
                    category   = @Category,
                    summary    = @Summary,
                    status     = @Status,
                    is_hero    = @IsHero,
                    is_trending = @IsTrending,
                    updated_at = @Now
                WHERE id = @Id", conn);

            BindParams(cmd, req, now);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();

            return Ok(FetchById(conn, id));
        }

        // ─── DELETE ───────────────────────────────────────────────
        /// <summary>DELETE /api/v1/admin/content/stories/:id</summary>
        [HttpDelete("stories/{id:int}")]
        public IActionResult DeleteStory(int id)
        {
            using var conn = OpenConnection();

            if (FetchById(conn, id) == null)
                return NotFound(new { message = "Story not found." });

            var cmd = new MySqlCommand("DELETE FROM content_stories WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();

            return Ok(new { message = "Story deleted successfully." });
        }

        // ─── PUBLISH ──────────────────────────────────────────────
        /// <summary>
        /// PATCH /api/v1/admin/content/stories/:id/publish
        /// Sets status = published. If the story is a hero, demotes the current hero first.
        /// </summary>
        [HttpPatch("stories/{id:int}/publish")]
        public IActionResult PublishStory(int id)
        {
            using var conn = OpenConnection();

            var story = FetchById(conn, id);
            if (story == null)
                return NotFound(new { message = "Story not found." });

            // If this story is marked as hero, demote any existing published hero
            if (story.IsHero)
                DemoteCurrentHero(conn, excludeId: id);

            SetStatus(conn, id, "published");
            return Ok(FetchById(conn, id));
        }

        // ─── UNPUBLISH ────────────────────────────────────────────
        /// <summary>
        /// PATCH /api/v1/admin/content/stories/:id/unpublish
        /// Sets status = draft.
        /// </summary>
        [HttpPatch("stories/{id:int}/unpublish")]
        public IActionResult UnpublishStory(int id)
        {
            using var conn = OpenConnection();

            if (FetchById(conn, id) == null)
                return NotFound(new { message = "Story not found." });

            SetStatus(conn, id, "draft");
            return Ok(FetchById(conn, id));
        }

        // ─── HELPERS ─────────────────────────────────────────────

        private MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Ensures only one hero can be published at a time.
        /// Demotes any currently published hero to draft (excluding the story being set as hero).
        /// </summary>
        private static void DemoteCurrentHero(MySqlConnection conn, int excludeId = 0)
        {
            var cmd = new MySqlCommand(@"
                UPDATE content_stories
                SET status = 'draft', updated_at = @Now
                WHERE is_hero = 1 AND status = 'published' AND id != @ExcludeId", conn);
            cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@ExcludeId", excludeId);
            cmd.ExecuteNonQuery();
        }

        private static void SetStatus(MySqlConnection conn, int id, string status)
        {
            var cmd = new MySqlCommand(@"
                UPDATE content_stories
                SET status = @Status, updated_at = @Now
                WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        private static ContentStory? FetchById(MySqlConnection conn, int id)
        {
            var cmd = new MySqlCommand(@"
                SELECT id, title, category, summary, status, is_hero, is_trending, created_at, updated_at
                FROM content_stories
                WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapRow(reader) : null;
        }

        private static void BindParams(MySqlCommand cmd, ContentStoryRequest req, DateTime now)
        {
            cmd.Parameters.AddWithValue("@Title",      req.Title);
            cmd.Parameters.AddWithValue("@Category",   req.Category ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Summary",    req.Summary ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Status",     req.Status);
            cmd.Parameters.AddWithValue("@IsHero",     req.IsHero);
            cmd.Parameters.AddWithValue("@IsTrending", req.IsTrending);
            cmd.Parameters.AddWithValue("@Now",        now);
        }

        private static ContentStory MapRow(MySqlDataReader r) => new ContentStory
        {
            Id         = r.GetInt32("id"),
            Title      = r.GetString("title"),
            Category   = r.IsDBNull(r.GetOrdinal("category")) ? null : r.GetString("category"),
            Summary    = r.IsDBNull(r.GetOrdinal("summary"))  ? null : r.GetString("summary"),
            Status     = r.GetString("status"),
            IsHero     = r.GetBoolean("is_hero"),
            IsTrending = r.GetBoolean("is_trending"),
            CreatedAt  = r.GetDateTime("created_at"),
            UpdatedAt  = r.GetDateTime("updated_at"),
        };

        private IActionResult? ValidateRequest(ContentStoryRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Title))
                return BadRequest(new { message = "title is required." });

            if (req.Status != "draft" && req.Status != "published")
                return BadRequest(new { message = "status must be 'draft' or 'published'." });

            return null;
        }
    }
}
