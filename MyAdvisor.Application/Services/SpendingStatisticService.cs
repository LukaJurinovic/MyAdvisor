using MyAdvisor.Application.DTOs.Statistics;
using MyAdvisor.Application.DTOs.Transaction;
using MyAdvisor.Application.DTOs.Category;
using MyAdvisor.Application.Interfaces.Services.App;
using MyAdvisor.Application.Interfaces.Services.Domain;

namespace MyAdvisor.Application.Services
{
    public class SpendingStatisticService : ISpendingStatisticService
    {
        private readonly IFinancialDiaryService _diaryService;
        private readonly ICategoryService _categoryService;

        public SpendingStatisticService(
            IFinancialDiaryService diaryService,
            ICategoryService categoryService)
        {
            _diaryService = diaryService;
            _categoryService = categoryService;
        }

        public async Task<SpendingStatisticDto> GetForMonthAsync(int userId, int year, int month)
        {
            var diaries = await _diaryService.GetAllWithTransactionsAsync(userId);
            var transactions = diaries
                .SelectMany(d => d.Transactions)
                .Where(t => t.TransactionDate.Year == year && t.TransactionDate.Month == month)
                .ToList();

            var categories = await _categoryService.GetAllAsync();
            var categoryNames = categories.ToDictionary(c => c.Id, c => c.Name);
            var incomeCategoryIds = GetIncomeCategoryIds(categories);

            return BuildStatistic(year, month, transactions, categoryNames, incomeCategoryIds);
        }

        public async Task<IReadOnlyList<SpendingStatisticDto>> GetForYearAsync(int userId, int year)
        {
            var diaries = await _diaryService.GetAllWithTransactionsAsync(userId);
            var transactions = diaries
                .SelectMany(d => d.Transactions)
                .Where(t => t.TransactionDate.Year == year)
                .ToList();

            var categories = await _categoryService.GetAllAsync();
            var categoryNames = categories.ToDictionary(c => c.Id, c => c.Name);
            var incomeCategoryIds = GetIncomeCategoryIds(categories);

            return Enumerable.Range(1, 12)
                .Select(month => BuildStatistic(
                    year,
                    month,
                    transactions.Where(t => t.TransactionDate.Month == month).ToList(),
                    categoryNames,
                    incomeCategoryIds))
                .ToList();
        }

        private static SpendingStatisticDto BuildStatistic(
            int year,
            int month,
            IReadOnlyList<TransactionDto> transactions,
            IReadOnlyDictionary<int, string> categoryNames,
            IReadOnlySet<int> incomeCategoryIds)
        {
            var spendingTransactions = transactions
                .Where(t => !IsIncomeCategory(t.CategoryId, incomeCategoryIds))
                .ToList();
            var incomeTransactions = transactions
                .Where(t => IsIncomeCategory(t.CategoryId, incomeCategoryIds))
                .ToList();

            var totalSpent = spendingTransactions.Sum(GetAbsoluteAmount);
            var totalIncome = incomeTransactions.Sum(GetAbsoluteAmount);

            var spendingBreakdown = BuildBreakdown(spendingTransactions, totalSpent, categoryNames);
            var incomeBreakdown = BuildBreakdown(incomeTransactions, totalIncome, categoryNames);

            return new SpendingStatisticDto(
                year,
                month,
                totalSpent,
                totalIncome,
                totalIncome - totalSpent,
                spendingBreakdown,
                incomeBreakdown);
        }

        private static IReadOnlyList<CategoryBreakdownDto> BuildBreakdown(
            IReadOnlyList<TransactionDto> transactions,
            decimal total,
            IReadOnlyDictionary<int, string> categoryNames)
            => transactions
                .GroupBy(t => t.CategoryId)
                .Select(g =>
                {
                    var categoryTotal = g.Sum(GetAbsoluteAmount);
                    return new CategoryBreakdownDto(
                        g.Key,
                        GetCategoryName(g.Key, categoryNames),
                        categoryTotal,
                        total == 0 ? 0 : Math.Round(categoryTotal / total * 100, 2));
                })
                .OrderByDescending(b => b.TotalAmount)
                .ThenBy(b => b.CategoryName)
                .ToList();

        private static decimal GetAbsoluteAmount(TransactionDto transaction)
            => Math.Abs(transaction.Amount);

        private static string GetCategoryName(int? categoryId, IReadOnlyDictionary<int, string> categoryNames)
        {
            if (categoryId is null)
                return "Uncategorized";

            return categoryNames.TryGetValue(categoryId.Value, out var name)
                ? name
                : "Unknown category";
        }

        private static bool IsIncomeCategory(int? categoryId, IReadOnlySet<int> incomeCategoryIds)
            => categoryId.HasValue && incomeCategoryIds.Contains(categoryId.Value);

        private static IReadOnlySet<int> GetIncomeCategoryIds(IReadOnlyList<CategoryDto> categories)
        {
            var incomeCategoryIds = categories
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
                        incomeCategoryIds.Contains(category.ParentCategoryId.Value) &&
                        incomeCategoryIds.Add(category.Id))
                    {
                        added = true;
                    }
                }
            }

            return incomeCategoryIds;
        }
    }
}
