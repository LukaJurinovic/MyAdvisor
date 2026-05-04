using MyAdvisor.Application.DTOs.Transaction;
using MyAdvisor.Application.Interfaces.Services.App;
using MyAdvisor.Application.Interfaces.Services.Domain;

namespace MyAdvisor.Application.Services
{
    public class DiaryTransactionService : IDiaryTransactionService
    {
        private readonly IFinancialDiaryService _diaryService;
        private readonly ITransactionService _transactionService;

        public DiaryTransactionService(
            IFinancialDiaryService diaryService,
            ITransactionService transactionService)
        {
            _diaryService = diaryService;
            _transactionService = transactionService;
        }

        public async Task<TransactionDto> GetByIdAsync(int id, int userId)
        {
            var transaction = await _transactionService.GetByIdAsync(id);

            var diary = await _diaryService.GetByIdAsync(transaction.DiaryId)
                ?? throw new KeyNotFoundException($"Diary {transaction.DiaryId} not found.");

            if (diary.UserId != userId)
                throw new UnauthorizedAccessException();

            return transaction;
        }

        public async Task<IReadOnlyList<TransactionDto>> GetByDiaryIdAsync(int diaryId, int userId)
        {
            var diary = await _diaryService.GetByIdAsync(diaryId)
                ?? throw new KeyNotFoundException($"Diary {diaryId} not found.");

            if (diary.UserId != userId)
                throw new UnauthorizedAccessException();

            return await _transactionService.GetByDiaryIdAsync(diaryId);
        }

        public Task<TransactionDto> AddAsync(AddTransactionRequestDto request, int userId)
            => _diaryService.AddTransactionAsync(request, userId);

        public async Task<TransactionDto> UpdateAsync(int transactionId, UpdateTransactionRequestDto request, int userId)
        {
            var transaction = await _transactionService.GetByIdAsync(transactionId);
            return await _diaryService.UpdateTransactionAsync(transaction.DiaryId, transactionId, request, userId);
        }

        public async Task DeleteAsync(int transactionId, int userId)
        {
            var transaction = await _transactionService.GetByIdAsync(transactionId);
            await _diaryService.DeleteTransactionAsync(transaction.DiaryId, transactionId, userId);
        }
    }
}
