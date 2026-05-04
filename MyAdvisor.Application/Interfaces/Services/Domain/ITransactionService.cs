using MyAdvisor.Application.DTOs.Transaction;

namespace MyAdvisor.Application.Interfaces.Services.Domain
{
    public interface ITransactionService
    {
        Task<TransactionDto> GetByIdAsync(int id);
        Task<IReadOnlyList<TransactionDto>> GetByDiaryIdAsync(int diaryId);
    }
}
