using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("search")]
    public class SearchController : ControllerBase
    {
        private readonly IConfiguration _config;

        public SearchController(IConfiguration config)
        {
            _config = config;
        }
        // Search published stories by keyword across title, content, and author
        [HttpGet]
        public IActionResult Search([FromQuery] string? q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { message = "Query parameter 'q' is required." });

            using var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            var cmd = new MySqlCommand(@"
                SELECT id, bucket, category, title, excerpt, content, author,
                       read_time_minutes, tone, is_pinned, pin_order,
                       status, published_at, created_at, updated_at
                FROM stories
                WHERE status = 'published'
                  AND (title LIKE @Query OR excerpt LIKE @Query OR content LIKE @Query OR author LIKE @Query)
                ORDER BY published_at DESC, created_at DESC
                LIMIT 50", conn);

            cmd.Parameters.AddWithValue("@Query", $"%{q}%");

            var results = new List<Story>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new Story
                {
                    Id = reader.GetInt32("id"),
                    Bucket = reader.GetString("bucket"),
                    Category = reader.IsDBNull(reader.GetOrdinal("category")) ? "" : reader.GetString("category"),
                    Title = reader.GetString("title"),
                    Excerpt = reader.IsDBNull(reader.GetOrdinal("excerpt")) ? "" : reader.GetString("excerpt"),
                    Content = reader.IsDBNull(reader.GetOrdinal("content")) ? "" : reader.GetString("content"),
                    Author = reader.IsDBNull(reader.GetOrdinal("author")) ? "" : reader.GetString("author"),
                    ReadTimeMinutes = reader.GetInt32("read_time_minutes"),
                    Tone = reader.IsDBNull(reader.GetOrdinal("tone")) ? "" : reader.GetString("tone"),
                    IsPinned = reader.GetBoolean("is_pinned"),
                    PinOrder = reader.GetInt32("pin_order"),
                    Status = reader.GetString("status"),
                    PublishedAt = reader.IsDBNull(reader.GetOrdinal("published_at")) ? null : reader.GetDateTime("published_at"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    UpdatedAt = reader.GetDateTime("updated_at"),
                });
            }

            return Ok(new { query = q, count = results.Count, results });
        }
    }
}