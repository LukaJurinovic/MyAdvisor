using MyAdvisor.Domain.Entities;

namespace MyAdvisor.Application.Interfaces.Repositories
{
    public interface ISpendingStatisticRepository
    {
        Task<SpendingStatistic?> GetByUserIdAndPeriodAsync(int userId, int month, int year);
        Task<IReadOnlyList<SpendingStatistic>> GetByUserIdAndYearAsync(int userId, int year);
        Task ReplaceAsync(SpendingStatistic statistic, int userId, int year, int month);
    }
}
