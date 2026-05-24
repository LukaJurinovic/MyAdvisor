using MyAdvisor.Application.DTOs.Statistics;

namespace MyAdvisor.Application.Interfaces.Services.App
{
    public interface ISpendingStatisticService
    {
        Task<SpendingStatisticDto> GetForMonthAsync(int userId, int year, int month);
        Task<IReadOnlyList<SpendingStatisticDto>> GetForYearAsync(int userId, int year);
    }
}
