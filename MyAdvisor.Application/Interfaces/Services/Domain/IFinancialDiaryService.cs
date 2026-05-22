using MyAdvisor.Application.DTOs.FinancialDiary;
using MyAdvisor.Application.DTOs.Transaction;

namespace MyAdvisor.Application.Interfaces.Services.Domain
{
    public interface IFinancialDiaryService
    {
        Task<FinancialDiaryDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<FinancialDiarySummaryDto>> GetAllAsync(int userId);
        Task<IReadOnlyList<FinancialDiaryDto>> GetAllWithTransactionsAsync(int userId);
        Task<FinancialDiaryDto> CreateAsync(CreateFinancialDiaryRequestDto request, int userId);
        Task<FinancialDiaryDto> UpdateAsync(int id, UpdateFinancialDiaryRequestDto request);
        Task DeleteAsync(int id);

        Task<int> EnsureDiaryExistsAsync(int userId, DateTime date);
        Task<TransactionDto> AddTransactionAsync(AddTransactionRequestDto request, int userId);
        Task<TransactionDto> UpdateTransactionAsync(int diaryId, int transactionId, UpdateTransactionRequestDto request, int userId);
        Task DeleteTransactionAsync(int diaryId, int transactionId, int userId);
    }
}
