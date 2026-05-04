using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MyAdvisor.Application.Interfaces.Services.AI;
using MyAdvisor.Infrastructure.AI;

namespace MyAdvisor.Infrastructure.Services.AI
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _http;
        private readonly string _model;

        public GeminiService(HttpClient http, IOptions<GeminiSettings> settings)
        {
            _http = http;
            _model = settings.Value.Model;
        }

        public async Task<string> AnalyzeImageAsync(byte[] imageData, string mimeType, string prompt)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { inline_data = new { mime_type = mimeType, data = Convert.ToBase64String(imageData) } },
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new { temperature = 0.1, maxOutputTokens = 2048 }
            };

            return StripMarkdownFences(await CallGeminiAsync(requestBody));
        }

        private static string StripMarkdownFences(string raw)
        {
            var cleaned = raw.Trim();
            if (!cleaned.StartsWith("```")) return cleaned;
            var firstNewline = cleaned.IndexOf('\n');
            if (firstNewline >= 0) cleaned = cleaned[(firstNewline + 1)..];
            var lastFence = cleaned.LastIndexOf("```");
            if (lastFence >= 0) cleaned = cleaned[..lastFence];
            return cleaned.Trim();
        }

        public async Task<string> ChatAsync(string systemPrompt, List<(string Role, string Content)> history, string userMessage)
        {
            var contents = new List<object>();

            foreach (var (role, content) in history)
            {
                contents.Add(new
                {
                    role = role == "assistant" ? "model" : "user",
                    parts = new[] { new { text = content } }
                });
            }

            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = userMessage } }
            });

            var requestBody = new
            {
                system_instruction = new { parts = new[] { new { text = systemPrompt } } },
                contents,
                generationConfig = new { temperature = 0.7, maxOutputTokens = 2048 }
            };

            return await CallGeminiAsync(requestBody);
        }

        private async Task<string> CallGeminiAsync(object requestBody)
        {
            var url = $"v1beta/models/{_model}:generateContent";
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(url, content);
            var rawJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Gemini API error ({(int)response.StatusCode}): {rawJson}");

            using var doc = JsonDocument.Parse(rawJson);
            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;
        }
    }
}
