using MyAdvisor.Client.Models.AI;
using MyAdvisor.Client.Models.Common;
using System.Net.Http.Json;

namespace MyAdvisor.Client.Services
{
    public class ChatService(HttpClient http)
    {
        public async Task<string> SendAsync(List<ChatMessage> history, string message, bool includeContext)
        {
            var res = await http.PostAsJsonAsync("/api/chat", new
            {
                history = history.Select(m => new { m.Role, m.Content }),
                message,
                includeFinancialContext = includeContext
            });

            await ThrowIfErrorAsync(res, "Chat request failed.");
            var response = await res.Content.ReadFromJsonAsync<ChatResponse>();
            return response?.Reply ?? string.Empty;
        }

        public async Task<string> SummarizeImageAsync(Stream imageStream, string fileName, string contentType)
        {
            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(imageStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(streamContent, "image", fileName);

            var res = await http.PostAsync("/api/chat/summarize-image", content);
            await ThrowIfErrorAsync(res, "Image summarization failed.");
            var response = await res.Content.ReadFromJsonAsync<ChatResponse>();
            return response?.Reply ?? string.Empty;
        }

        private static async Task ThrowIfErrorAsync(HttpResponseMessage res, string fallback)
        {
            if (res.IsSuccessStatusCode) return;
            var data = await res.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new Exception(data?.Error ?? fallback);
        }
    }
}
