namespace MyAdvisor.Application.Interfaces.Services.AI
{
    public interface IGeminiService
    {
        Task<string> AnalyzeImageAsync(byte[] imageData, string mimeType, string prompt);
        Task<string> ChatAsync(string systemPrompt, List<(string Role, string Content)> history, string userMessage);
    }
}
