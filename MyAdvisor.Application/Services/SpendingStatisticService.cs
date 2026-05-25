using MyAdvisor.Application.DTOs.Statistics;
using MyAdvisor.Application.Interfaces.Repositories;
using MyAdvisor.Application.Interfaces.Services.Domain;
using MyAdvisor.Domain.Entities;

namespace MyAdvisor.Application.Services
{
    public class SpendingStatisticService : ISpendingStatisticService
    {
        private readonly ISpendingStatisticRepository _repository;

        public SpendingStatisticService(ISpendingStatisticRepository repository)
        {
            _repository = repository;
        }

        public async Task<SpendingStatisticDto> GetForMonthAsync(int userId, int year, int month)
        {
            var stat = await _repository.GetByUserIdAndPeriodAsync(userId, month, year);
            return stat is null ? EmptyDto(year, month) : MapToDto(stat);
        }

        public async Task<IReadOnlyList<SpendingStatisticDto>> GetForYearAsync(int userId, int year)
        {
            var stats = await _repository.GetByUserIdAndYearAsync(userId, year);
            var byMonth = stats.ToDictionary(s => s.Month);
            return Enumerable.Range(1, 12)
                .Select(month => byMonth.TryGetValue(month, out var s) ? MapToDto(s) : EmptyDto(year, month))
                .ToList();
        }

        public async Task UpsertAsync(int userId, int year, int month, decimal totalSpent, decimal totalIncome,
            IReadOnlyList<CategoryBreakdownDto> spending, IReadOnlyList<CategoryBreakdownDto> income)
        {
            var stat = new SpendingStatistic(userId, month, year, totalSpent, totalIncome);
            foreach (var item in spending)
                stat.AddCategoryStatistic(new CategoryStatistic(stat, item.CategoryId, item.CategoryName, item.TotalAmount, item.Percentage, isIncome: false));
            foreach (var item in income)
                stat.AddCategoryStatistic(new CategoryStatistic(stat, item.CategoryId, item.CategoryName, item.TotalAmount, item.Percentage, isIncome: true));

            await _repository.ReplaceAsync(stat, userId, year, month);
        }

        private static SpendingStatisticDto MapToDto(SpendingStatistic stat)
        {
            var spending = stat.CategoryBreakdown
                .Where(c => !c.IsIncome)
                .Select(c => new CategoryBreakdownDto(c.CategoryId, c.CategoryName, c.TotalAmount, c.Percentage))
                .OrderByDescending(c => c.TotalAmount)
                .ToList();
            var income = stat.CategoryBreakdown
                .Where(c => c.IsIncome)
                .Select(c => new CategoryBreakdownDto(c.CategoryId, c.CategoryName, c.TotalAmount, c.Percentage))
                .OrderByDescending(c => c.TotalAmount)
                .ToList();
            return new SpendingStatisticDto(stat.Year, stat.Month, stat.TotalSpent, stat.TotalIncome,
                stat.TotalIncome - stat.TotalSpent, spending, income);
        }

        private static SpendingStatisticDto EmptyDto(int year, int month)
            => new(year, month, 0, 0, 0, [], []);
    }
}
