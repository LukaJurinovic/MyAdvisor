using MyAdvisor.Domain.Entities;

namespace MyAdvisor.Application.Interfaces.Repositories
{
    public interface IFinancialDiaryRepository
    {
        Task<FinancialDiary?> GetByIdAsync(int id);
        Task<FinancialDiary?> GetByIdWithTransactionsAsync(int id);
        Task<IReadOnlyList<FinancialDiary>> GetByUserIdAsync(int userId);
        Task<IReadOnlyList<FinancialDiary>> GetByUserIdWithTransactionsAsync(int userId);
        Task<FinancialDiary?> GetByUserIdAndDateAsync(int userId, DateTime date);
        Task<IReadOnlyList<int>> GetDistinctUserIdsAsync();
        Task AddAsync(FinancialDiary diary);
        Task UpdateAsync(FinancialDiary diary);
        Task DeleteAsync(int id);
    }
}
