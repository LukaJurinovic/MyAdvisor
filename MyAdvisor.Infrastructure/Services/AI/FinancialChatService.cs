using MyAdvisor.Application.DTOs.AI;
using MyAdvisor.Application.Interfaces.Services.AI;
using MyAdvisor.Application.Interfaces.Services.App;
using MyAdvisor.Application.Interfaces.Services.Domain;

namespace MyAdvisor.Infrastructure.Services.AI
{
    public class FinancialChatService : IFinancialChatService
    {
        private readonly IGeminiService _gemini;
        private readonly IFinancialDiaryService _diaryService;
        private readonly ICategoryService _categoryService;

        public FinancialChatService(
            IGeminiService gemini,
            IFinancialDiaryService diaryService,
            ICategoryService categoryService)
        {
            _gemini = gemini;
            _diaryService = diaryService;
            _categoryService = categoryService;
        }

        public async Task<ChatResponseDto> ChatAsync(int userId, ChatRequestDto request)
        {
            var systemPrompt = await BuildSystemPromptAsync(userId, request.IncludeFinancialContext);
            var history = request.History.Select(m => (m.Role, m.Content)).ToList();
            var reply = await _gemini.ChatAsync(systemPrompt, history, request.Message);
            return new ChatResponseDto(reply);
        }

        public async Task<ChatResponseDto> SummarizeImageAsync(byte[] imageData, string mimeType)
        {
            var prompt = """
                You are a financial analyst reviewing a receipt, bank statement, or financial document.
                Provide a clear, structured summary covering:
                1. What type of document this is
                2. Key amounts and what they represent
                3. Merchant/vendor details if visible
                4. Date and payment method if visible
                5. Any notable observations or insights about the spending

                Be concise and practical. Format your response clearly.
                """;

            var reply = await _gemini.AnalyzeImageAsync(imageData, mimeType, prompt);
            return new ChatResponseDto(reply);
        }

        private async Task<string> BuildSystemPromptAsync(int userId, bool includeContext)
        {
            var basePrompt = """
                You are a knowledgeable and friendly personal financial advisor built into MyAdvisor.
                You help users understand their spending habits, give budgeting advice, explain financial concepts,
                and answer any questions about banking, saving, investing, or money management.
                Be concise, practical, and supportive. Use clear language and avoid jargon unless asked.
                """;

            if (!includeContext)
                return basePrompt;

            var diaries = await _diaryService.GetAllWithTransactionsAsync(userId);
            var categories = await _categoryService.GetAllAsync();
            var catMap = categories.ToDictionary(c => c.Id, c => c.Name);

            var allTransactions = diaries
                .SelectMany(d => d.Transactions)
                .Select(tx => (
                    Date: tx.TransactionDate.ToString("yyyy-MM-dd"),
                    tx.Amount,
                    tx.Description,
                    Category: tx.CategoryId.HasValue && catMap.TryGetValue(tx.CategoryId.Value, out var cat) ? cat : null
                ))
                .ToList();

            if (allTransactions.Count == 0)
                return basePrompt + "\n\nThe user has no transactions recorded yet.";

            var total = allTransactions.Sum(t => t.Amount);
            var expenses = allTransactions.Where(t => t.Amount < 0).Sum(t => t.Amount);
            var income = allTransactions.Where(t => t.Amount > 0).Sum(t => t.Amount);

            var byCategory = allTransactions
                .Where(t => t.Amount < 0 && t.Category != null)
                .GroupBy(t => t.Category!)
                .Select(g => $"{g.Key}: {g.Sum(t => t.Amount):N2}€")
                .ToList();

            var recentTx = allTransactions
                .OrderByDescending(t => t.Date)
                .Take(20)
                .Select(t => $"{t.Date} | {(t.Amount >= 0 ? "+" : "")}{t.Amount:N2}€ | {t.Description ?? "—"} | {t.Category ?? "Uncategorized"}");

            var context = $"""

                --- USER FINANCIAL SUMMARY ---
                Total balance across all diaries: {total:N2}€
                Total income: +{income:N2}€
                Total expenses: {expenses:N2}€

                Spending by category:
                {string.Join("\n", byCategory)}

                Recent transactions (up to 20):
                {string.Join("\n", recentTx)}
                --- END OF SUMMARY ---

                Use this data to give personalized, specific advice when relevant.
                """;

            return basePrompt + context;
        }
    }
}
