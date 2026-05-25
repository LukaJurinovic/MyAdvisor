using MyAdvisor.Application.DTOs.Statistics;

namespace MyAdvisor.Application.Interfaces.Services.Domain
{
    public interface ISpendingStatisticService
    {
        Task<SpendingStatisticDto> GetForMonthAsync(int userId, int year, int month);
        Task<IReadOnlyList<SpendingStatisticDto>> GetForYearAsync(int userId, int year);
        Task UpsertAsync(int userId, int year, int month, decimal totalSpent, decimal totalIncome,
            IReadOnlyList<CategoryBreakdownDto> spending, IReadOnlyList<CategoryBreakdownDto> income);
    }
}
