using System.Text;
using System.Text.Json;

namespace WebApplication1.Services
{
    /// <summary>
    /// Wraps the DeepSeek API (OpenAI-compatible chat/completions endpoint).
    /// Configured via appsettings: Ai:ApiKey, Ai:Model, Ai:MaxTokens, Ai:SystemPrompt
    /// </summary>
    public class AiService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<AiService> _logger;

        public AiService(HttpClient http, IConfiguration config, ILogger<AiService> logger)
        {
            _http = http;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Sends a conversation history to DeepSeek and returns the assistant reply text.
        /// </summary>
        public async Task<string> ChatAsync(
            IEnumerable<AiMessage> messages,
            CancellationToken cancellationToken = default)
        {
            var aiSettings = _config.GetSection("Ai");
            var model      = aiSettings["Model"]     ?? "deepseek-chat";
            var maxTokens  = int.Parse(aiSettings["MaxTokens"] ?? "1024");
            var apiKey     = aiSettings["ApiKey"]    ?? "";
            var systemPrompt = aiSettings["SystemPrompt"]
                ?? "You are a helpful news assistant for a college news portal. Answer questions about news, summarise stories, and help users find relevant content. Be concise and factual.";

            const string endpoint = "https://api.deepseek.com/chat/completions";

            // Build messages array — prepend system message
            var messageList = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };
            messageList.AddRange(messages.Select(m => new { role = m.Role, content = m.Content }));

            var payload = new
            {
                model,
                messages  = messageList,
                max_tokens = maxTokens
            };

            var json    = JsonSerializer.Serialize(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Authorization", $"Bearer {apiKey}");

            HttpResponseMessage response;
            try
            {
                response = await _http.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTTP error calling DeepSeek API");
                throw new InvalidOperationException("AI provider unreachable.", ex);
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("DeepSeek API error {Status}: {Body}", response.StatusCode, responseBody);
                throw new InvalidOperationException($"AI provider returned {(int)response.StatusCode}.");
            }

            // Parse OpenAI-compatible response:
            // { choices: [ { message: { role: "assistant", content: "..." } } ] }
            using var doc = JsonDocument.Parse(responseBody);
            var choices = doc.RootElement.GetProperty("choices");

            if (choices.GetArrayLength() > 0)
            {
                return choices[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? string.Empty;
            }

            return string.Empty;
        }
    }

    /// <summary>A single chat message sent to / received from the AI.</summary>
    public record AiMessage(string Role, string Content);
}
