using MyAdvisor.Domain.Entities;

namespace MyAdvisor.Application.Interfaces.Repositories
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(int id);
        Task<IReadOnlyList<Transaction>> GetByDiaryIdAsync(int diaryId);
    }
}
