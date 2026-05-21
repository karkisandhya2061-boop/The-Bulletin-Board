using System.Collections.Concurrent;

namespace WebApplication1.Middleware
{
    /// <summary>
    /// Simple sliding-window rate limiter for the /chat/message endpoint.
    /// Limits by authenticated user ID if present, otherwise by IP.
    /// Config keys: RateLimit:ChatWindowSeconds, RateLimit:ChatMaxRequests
    /// </summary>
    public class ChatRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ChatRateLimitMiddleware> _logger;
        private readonly int _windowSeconds;
        private readonly int _maxRequests;

        // key → sorted list of request timestamps
        private static readonly ConcurrentDictionary<string, Queue<DateTime>> _windows = new();

        public ChatRateLimitMiddleware(
            RequestDelegate next,
            IConfiguration config,
            ILogger<ChatRateLimitMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _windowSeconds = int.Parse(config["RateLimit:ChatWindowSeconds"] ?? "60");
            _maxRequests = int.Parse(config["RateLimit:ChatMaxRequests"] ?? "30");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only rate-limit the chat message endpoint
            if (!context.Request.Path.StartsWithSegments("/chat/message"))
            {
                await _next(context);
                return;
            }

            var key = ResolveKey(context);
            var now = DateTime.UtcNow;

            var queue = _windows.GetOrAdd(key, _ => new Queue<DateTime>());

            bool rateLimited = false;

            lock (queue)
            {
                // Evict entries outside the window
                while (queue.Count > 0 && (now - queue.Peek()).TotalSeconds > _windowSeconds)
                    queue.Dequeue();

                if (queue.Count >= _maxRequests)
                {
                    _logger.LogWarning("Rate limit hit for key {Key}", key);
                    rateLimited = true;
                }
                else
                {
                    queue.Enqueue(now);
                }
            }

            if (rateLimited)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                       $"{{\"message\":\"Too many requests...\"}}");
                return;
            }

            await _next(context);
        }

        private static string ResolveKey(HttpContext ctx)
        {
            // Prefer authenticated user ID
            var userId = ctx.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId)) return $"user:{userId}";

            // Fall back to IP
            var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return $"ip:{ip}";
        }
    }

    // Extension for clean registration in Program.cs
    public static class ChatRateLimitMiddlewareExtensions
    {
        public static IApplicationBuilder UseChatRateLimit(this IApplicationBuilder app)
            => app.UseMiddleware<ChatRateLimitMiddleware>();
    }
}