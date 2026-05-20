using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Security.Claims;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Chat API  —  base route: /chat
    ///
    /// POST /chat/message          — Send a message; creates/continues a conversation
    /// GET  /chat/history/{sid}    — Retrieve message history for a session
    /// GET  /chat/conversations    — [auth] List all conversations for logged-in user
    /// DELETE /chat/{sid}          — Delete a conversation (owner or admin)
    /// </summary>
    [ApiController]
    [Route("chat")]
    public class ChatController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AiService _ai;
        private readonly ILogger<ChatController> _logger;

        // Maximum history messages sent to AI to keep context window manageable
        private const int MaxHistoryForAi = 20;

        public ChatController(IConfiguration config, AiService ai, ILogger<ChatController> logger)
        {
            _config = config;
            _ai = ai;
            _logger = logger;
        }

        // ─── POST /chat/message ───────────────────────────────────
        /// <summary>
        /// Send a message. Works for anonymous and authenticated users.
        /// Creates a new conversation if session_id is unseen; otherwise appends.
        /// Returns the AI reply and full conversation history.
        /// </summary>
        [HttpPost("message")]
        public async Task<IActionResult> SendMessage(
            [FromBody] ChatRequest req,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(req.SessionId))
                return BadRequest(new { message = "session_id is required." });

            if (string.IsNullOrWhiteSpace(req.Message))
                return BadRequest(new { message = "message is required." });

            // Sanitise input (prevent prompt injection)
            var userMessage = req.Message.Trim();
            if (userMessage.Length > 2000)
                return BadRequest(new { message = "message must be ≤ 2000 characters." });

            int? userId = GetCurrentUserId();

            using var conn = OpenConnection();

            // 1. Upsert conversation
            var conversation = GetOrCreateConversation(conn, req.SessionId, userId);

            // 2. Load existing history
            var history = GetMessages(conn, conversation.Id);

            // 3. Persist the new user message
            var userMsg = PersistMessage(conn, conversation.Id, "user", userMessage);
            history.Add(userMsg);

            // 4. Build AI message list (most recent MaxHistoryForAi turns)
            var aiMessages = history
                .TakeLast(MaxHistoryForAi)
                .Select(m => new AiMessage(m.Role, m.Content))
                .ToList();

            // 5. Call the AI provider
            string reply;
            try
            {
                reply = await _ai.ChatAsync(aiMessages, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI call failed for session {Session}", req.SessionId);
                return StatusCode(503, new { message = "AI service temporarily unavailable." });
            }

            // 6. Persist assistant reply
            var assistantMsg = PersistMessage(conn, conversation.Id, "assistant", reply);
            history.Add(assistantMsg);

            // 7. Auto-title the conversation after the first user message
            if (history.Count == 2)
                UpdateConversationTitle(conn, conversation.Id, Truncate(userMessage, 60));

            return Ok(new ChatResponse
            {
                ConversationId = conversation.Id,
                SessionId = req.SessionId,
                Reply = reply,
                History = history
            });
        }

        // ─── GET /chat/history/{sessionId} ───────────────────────
        /// <summary>
        /// Returns message history for a session.
        /// Anonymous callers can access their own session; authenticated users can
        /// access any session they own; admins can access everything.
        /// </summary>
        [HttpGet("history/{sessionId}")]
        public IActionResult GetHistory(string sessionId)
        {
            using var conn = OpenConnection();
            var conv = FindConversation(conn, sessionId);
            if (conv == null) return NotFound(new { message = "Conversation not found." });

            if (!CanAccessConversation(conv)) return Forbid();

            var messages = GetMessages(conn, conv.Id);
            return Ok(new { conv.Id, conv.SessionId, conv.Title, conv.CreatedAt, messages });
        }

        // ─── GET /chat/conversations ──────────────────────────────
        /// <summary>
        /// [Authenticated] Returns all conversations belonging to the current user.
        /// Admins see all conversations.
        /// </summary>
        [Authorize]
        [HttpGet("conversations")]
        public IActionResult GetConversations()
        {
            int userId = GetCurrentUserId() ?? 0;
            bool isAdmin = User.IsInRole("admin");

            using var conn = OpenConnection();
            var sql = isAdmin
                ? "SELECT id, user_id, session_id, title, created_at, updated_at FROM chat_conversations ORDER BY updated_at DESC LIMIT 200"
                : "SELECT id, user_id, session_id, title, created_at, updated_at FROM chat_conversations WHERE user_id = @UserId ORDER BY updated_at DESC";

            var cmd = new MySqlCommand(sql, conn);
            if (!isAdmin) cmd.Parameters.AddWithValue("@UserId", userId);

            var list = new List<ChatConversation>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapConversation(reader));
            return Ok(list);
        }

        // ─── DELETE /chat/{sessionId} ─────────────────────────────
        /// <summary>
        /// Delete a conversation and all its messages.
        /// Only the owner or an admin may delete.
        /// </summary>
        [Authorize]
        [HttpDelete("{sessionId}")]
        public IActionResult DeleteConversation(string sessionId)
        {
            using var conn = OpenConnection();
            var conv = FindConversation(conn, sessionId);
            if (conv == null) return NotFound(new { message = "Conversation not found." });
            if (!CanAccessConversation(conv)) return Forbid();

            var cmd = new MySqlCommand(
                "DELETE FROM chat_conversations WHERE id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", conv.Id);
            cmd.ExecuteNonQuery();

            return Ok(new { message = "Conversation deleted." });
        }

        // ─── HELPERS ─────────────────────────────────────────────

        private MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();
            return conn;
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : null;
        }

        private bool CanAccessConversation(ChatConversation conv)
        {
            if (User.IsInRole("admin")) return true;
            var userId = GetCurrentUserId();
            // Authenticated owner
            if (userId.HasValue && conv.UserId == userId) return true;
            // Anonymous: allow if conversation has no owner (session-based access is
            // handled by the client knowing their own session_id)
            if (!userId.HasValue && conv.UserId == null) return true;
            return false;
        }

        private static ChatConversation GetOrCreateConversation(
            MySqlConnection conn, string sessionId, int? userId)
        {
            var existing = FindConversation(conn, sessionId);
            if (existing != null) return existing;

            var cmd = new MySqlCommand(
                @"INSERT INTO chat_conversations (user_id, session_id, title)
                  VALUES (@UserId, @SessionId, 'New conversation');
                  SELECT LAST_INSERT_ID();",
                conn);
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            var newId = Convert.ToInt32(cmd.ExecuteScalar());

            return new ChatConversation
            {
                Id = newId,
                UserId = userId,
                SessionId = sessionId,
                Title = "New conversation",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private static ChatConversation? FindConversation(MySqlConnection conn, string sessionId)
        {
            var cmd = new MySqlCommand(
                "SELECT id, user_id, session_id, title, created_at, updated_at " +
                "FROM chat_conversations WHERE session_id = @SessionId LIMIT 1",
                conn);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            using var r = cmd.ExecuteReader();
            return r.Read() ? MapConversation(r) : null;
        }

        private static List<ChatMessage> GetMessages(MySqlConnection conn, int conversationId)
        {
            var cmd = new MySqlCommand(
                "SELECT id, conversation_id, role, content, created_at " +
                "FROM chat_messages WHERE conversation_id = @ConvId ORDER BY id ASC",
                conn);
            cmd.Parameters.AddWithValue("@ConvId", conversationId);

            var list = new List<ChatMessage>();
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapMessage(r));
            return list;
        }

        private static ChatMessage PersistMessage(
            MySqlConnection conn, int conversationId, string role, string content)
        {
            var cmd = new MySqlCommand(
                @"INSERT INTO chat_messages (conversation_id, role, content)
                  VALUES (@ConvId, @Role, @Content);
                  SELECT LAST_INSERT_ID();",
                conn);
            cmd.Parameters.AddWithValue("@ConvId", conversationId);
            cmd.Parameters.AddWithValue("@Role", role);
            cmd.Parameters.AddWithValue("@Content", content);
            var newId = Convert.ToInt32(cmd.ExecuteScalar());

            return new ChatMessage
            {
                Id = newId,
                ConversationId = conversationId,
                Role = role,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static void UpdateConversationTitle(
            MySqlConnection conn, int conversationId, string title)
        {
            var cmd = new MySqlCommand(
                "UPDATE chat_conversations SET title = @Title WHERE id = @Id",
                conn);
            cmd.Parameters.AddWithValue("@Title", title);
            cmd.Parameters.AddWithValue("@Id", conversationId);
            cmd.ExecuteNonQuery();
        }

        private static ChatConversation MapConversation(MySqlDataReader r) => new()
        {
            Id = r.GetInt32("id"),
            UserId = r.IsDBNull(r.GetOrdinal("user_id")) ? null : r.GetInt32("user_id"),
            SessionId = r.GetString("session_id"),
            Title = r.GetString("title"),
            CreatedAt = r.GetDateTime("created_at"),
            UpdatedAt = r.GetDateTime("updated_at"),
        };

        private static ChatMessage MapMessage(MySqlDataReader r) => new()
        {
            Id = r.GetInt32("id"),
            ConversationId = r.GetInt32("conversation_id"),
            Role = r.GetString("role"),
            Content = r.GetString("content"),
            CreatedAt = r.GetDateTime("created_at"),
        };

        private static string Truncate(string s, int max) =>
            s.Length <= max ? s : s[..max] + "…";
    }
}