using Microsoft.Extensions.Logging;
using MyAdvisor.Application.DTOs.Category;
using MyAdvisor.Application.DTOs.Statistics;
using MyAdvisor.Application.DTOs.Transaction;
using MyAdvisor.Application.Interfaces.Services.App;
using MyAdvisor.Application.Interfaces.Services.Domain;

namespace MyAdvisor.Application.Services
{
    public class SpendingStatisticComputeService : ISpendingStatisticComputeService
    {
        private readonly IFinancialDiaryService _diaryService;
        private readonly ICategoryService _categoryService;
        private readonly ISpendingStatisticService _statsService;
        private readonly ILogger<SpendingStatisticComputeService> _logger;

        public SpendingStatisticComputeService(
            IFinancialDiaryService diaryService,
            ICategoryService categoryService,
            ISpendingStatisticService statsService,
            ILogger<SpendingStatisticComputeService> logger)
        {
            _diaryService = diaryService;
            _categoryService = categoryService;
            _statsService = statsService;
            _logger = logger;
        }

        public async Task RecomputeForUserAsync(int userId)
        {
            var diaries = await _diaryService.GetAllWithTransactionsAsync(userId);
            var transactions = diaries.SelectMany(d => d.Transactions).ToList();
            _logger.LogInformation("RecomputeForUser {UserId}: found {DiaryCount} diaries, {TxCount} transactions.",
                userId, diaries.Count, transactions.Count);
            if (transactions.Count == 0) return;

            var categories = await _categoryService.GetAllAsync();
            var categoryNames = categories.ToDictionary(c => c.Id, c => c.Name);
            var incomeCategoryIds = GetIncomeCategoryIds(categories);

            var periods = transactions
                .Select(t => (t.TransactionDate.Year, t.TransactionDate.Month))
                .Distinct();

            foreach (var (year, month) in periods)
            {
                var periodTransactions = transactions
                    .Where(t => t.TransactionDate.Year == year && t.TransactionDate.Month == month)
                    .ToList();

                var (totalSpent, totalIncome, spending, income) = ComputeStats(
                    periodTransactions, categoryNames, incomeCategoryIds);

                await _statsService.UpsertAsync(userId, year, month, totalSpent, totalIncome, spending, income);
            }
        }

        private static (decimal totalSpent, decimal totalIncome,
            IReadOnlyList<CategoryBreakdownDto> spending, IReadOnlyList<CategoryBreakdownDto> income)
            ComputeStats(IReadOnlyList<TransactionDto> transactions,
                IReadOnlyDictionary<int, string> categoryNames,
                IReadOnlySet<int> incomeCategoryIds)
        {
            var spendingTx = transactions.Where(t => !IsIncome(t.CategoryId, incomeCategoryIds)).ToList();
            var incomeTx = transactions.Where(t => IsIncome(t.CategoryId, incomeCategoryIds)).ToList();

            var totalSpent = Math.Max(0, spendingTx.Sum(t => -t.Amount));
            var totalIncome = Math.Max(0, incomeTx.Sum(t => t.Amount));

            return (totalSpent, totalIncome,
                BuildBreakdown(spendingTx, totalSpent, categoryNames),
                BuildBreakdown(incomeTx, totalIncome, categoryNames));
        }

        private static IReadOnlyList<CategoryBreakdownDto> BuildBreakdown(
            IReadOnlyList<TransactionDto> transactions, decimal total,
            IReadOnlyDictionary<int, string> categoryNames)
            => transactions
                .GroupBy(t => t.CategoryId)
                .Select(g =>
                {
                    var categoryTotal = Math.Abs(g.Sum(t => t.Amount));
                    var percentage = total == 0 ? 0 : Math.Round(categoryTotal / total * 100, 2);
                    return new CategoryBreakdownDto(g.Key, ResolveCategoryName(g.Key, categoryNames), categoryTotal, percentage);
                })
                .OrderByDescending(b => b.TotalAmount)
                .ToList();

        private static string ResolveCategoryName(int? categoryId, IReadOnlyDictionary<int, string> categoryNames)
        {
            if (categoryId is null) return "Uncategorized";
            return categoryNames.TryGetValue(categoryId.Value, out var name) ? name : "Unknown category";
        }

        private static bool IsIncome(int? categoryId, IReadOnlySet<int> incomeCategoryIds)
            => categoryId.HasValue && incomeCategoryIds.Contains(categoryId.Value);

        private static IReadOnlySet<int> GetIncomeCategoryIds(IReadOnlyList<CategoryDto> categories)
        {
            var ids = categories
                .Where(c => string.Equals(c.Name, "Income", StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Id)
                .ToHashSet();

            var added = true;
            while (added)
            {
                added = false;
                foreach (var category in categories)
                {
                    if (category.ParentCategoryId.HasValue &&
                        ids.Contains(category.ParentCategoryId.Value) &&
                        ids.Add(category.Id))
                        added = true;
                }
            }

            return ids;
        }
    }
}
