using System.Text;
using System.Text.Json;

namespace WebApplication1.Services
{
    /// <summary>
    /// Wraps the Google Gemini API (generateContent endpoint).
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
        /// Sends a conversation history to Gemini and returns the assistant reply text.
        /// </summary>
        public async Task<string> ChatAsync(
            IEnumerable<AiMessage> messages,
            CancellationToken cancellationToken = default)
        {
            var aiSettings = _config.GetSection("Ai");
            var model = aiSettings["Model"] ?? "gemini-1.5-flash";
            var maxTokens = int.Parse(aiSettings["MaxTokens"] ?? "1024");
            var apiKey = aiSettings["ApiKey"] ?? "";
            var systemPrompt = aiSettings["SystemPrompt"]
                ?? "You are a helpful news assistant for a college news portal. Answer questions about news, summarise stories, and help users find relevant content. Be concise and factual.";

            // Gemini endpoint — API key passed as query param
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            // Gemini uses "user" and "model" roles (not "assistant")
            var geminiContents = messages.Select(m => new
            {
                role = m.Role == "assistant" ? "model" : "user",
                parts = new[] { new { text = m.Content } }
            });

            var payload = new
            {
                system_instruction = new
                {
                    parts = new[] { new { text = systemPrompt } }
                },
                contents = geminiContents,
                generationConfig = new { maxOutputTokens = maxTokens }
            };

            var json = JsonSerializer.Serialize(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response;
            try
            {
                response = await _http.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTTP error calling Gemini API");
                throw new InvalidOperationException("AI provider unreachable.", ex);
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API error {Status}: {Body}", response.StatusCode, responseBody);
                throw new InvalidOperationException($"AI provider returned {(int)response.StatusCode}.");
            }

            // Parse: { candidates: [ { content: { parts: [ { text: "..." } ] } } ] }
            using var doc = JsonDocument.Parse(responseBody);
            var candidates = doc.RootElement.GetProperty("candidates");

            if (candidates.GetArrayLength() > 0)
            {
                var parts = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts");

                if (parts.GetArrayLength() > 0)
                    return parts[0].GetProperty("text").GetString() ?? string.Empty;
            }

            return string.Empty;
        }
    }

    /// <summary>A single chat message sent to / received from the AI.</summary>
    public record AiMessage(string Role, string Content);
}