namespace MyAdvisor.Client.Models.AI
{
    public class ChatMessage
    {
        public string Role { get; set; } = "user";
        public string Content { get; set; } = string.Empty;
    }

    public class ChatRequest
    {
        public List<ChatMessage> History { get; set; } = [];
        public string Message { get; set; } = string.Empty;
        public bool IncludeFinancialContext { get; set; } = true;
    }

    public class ChatResponse
    {
        public string Reply { get; set; } = string.Empty;
    }
}
