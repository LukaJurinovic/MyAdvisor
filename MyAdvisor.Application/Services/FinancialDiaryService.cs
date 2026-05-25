using MyAdvisor.Application.DTOs.FinancialDiary;
using MyAdvisor.Application.DTOs.Transaction;
using MyAdvisor.Application.Interfaces.Repositories;
using MyAdvisor.Application.Interfaces.Services.Domain;
using MyAdvisor.Application.Mappers;
using MyAdvisor.Domain.Entities;

namespace MyAdvisor.Application.Services
{
    public class FinancialDiaryService : IFinancialDiaryService
    {
        private readonly IFinancialDiaryRepository _diaryRepository;
        private readonly FinancialDiaryMapper _mapper;
        private readonly TransactionMapper _transactionMapper;

        public FinancialDiaryService(
            IFinancialDiaryRepository diaryRepository,
            FinancialDiaryMapper mapper,
            TransactionMapper transactionMapper)
        {
            _diaryRepository = diaryRepository;
            _mapper = mapper;
            _transactionMapper = transactionMapper;
        }

        public async Task<FinancialDiaryDto?> GetByIdAsync(int id)
        {
            var diary = await _diaryRepository.GetByIdWithTransactionsAsync(id);
            return diary is null ? null : _mapper.ToDto(diary);
        }

        public async Task<IReadOnlyList<FinancialDiarySummaryDto>> GetAllAsync(int userId)
        {
            var diaries = await _diaryRepository.GetByUserIdAsync(userId);
            return diaries.Select(_mapper.ToSummaryDto).ToList();
        }

        public async Task<IReadOnlyList<FinancialDiaryDto>> GetAllWithTransactionsAsync(int userId)
        {
            var diaries = await _diaryRepository.GetByUserIdWithTransactionsAsync(userId);
            return diaries.Select(_mapper.ToDto).ToList();
        }

        public async Task<IReadOnlyList<int>> GetDistinctUserIdsAsync()
            => await _diaryRepository.GetDistinctUserIdsAsync();

        public async Task<FinancialDiaryDto> CreateAsync(CreateFinancialDiaryRequestDto request, int userId)
        {
            var diary = new FinancialDiary(userId, request.Date, request.Notes);
            await _diaryRepository.AddAsync(diary);
            return _mapper.ToDto(diary);
        }

        public async Task<FinancialDiaryDto> UpdateAsync(int id, UpdateFinancialDiaryRequestDto request)
        {
            var diary = await _diaryRepository.GetByIdWithTransactionsAsync(id)
                ?? throw new KeyNotFoundException($"Diary {id} not found.");

            diary.UpdateNotes(request.Notes);
            await _diaryRepository.UpdateAsync(diary);
            return _mapper.ToDto(diary);
        }

        public async Task DeleteAsync(int id)
        {
            await _diaryRepository.DeleteAsync(id);
        }

        public async Task<int> EnsureDiaryExistsAsync(int userId, DateTime date)
        {
            var existing = await _diaryRepository.GetByUserIdAndDateAsync(userId, date.Date);
            if (existing is not null) return existing.Id;

            var diary = new FinancialDiary(userId, date.Date);
            await _diaryRepository.AddAsync(diary);
            return diary.Id;
        }

        public async Task<TransactionDto> AddTransactionAsync(AddTransactionRequestDto request, int userId)
        {
            var diary = await _diaryRepository.GetByIdWithTransactionsAsync(request.DiaryId)
                ?? throw new KeyNotFoundException($"Diary {request.DiaryId} not found.");

            if (diary.UserId != userId)
                throw new UnauthorizedAccessException();

            var transaction = new Transaction(
                request.DiaryId,
                request.Amount,
                request.CategoryId,
                request.Description,
                request.TransactionDate,
                request.PaymentMethod);

            diary.AddTransaction(transaction);
            await _diaryRepository.UpdateAsync(diary);
            return _transactionMapper.ToDto(transaction);
        }

        public async Task<TransactionDto> UpdateTransactionAsync(int diaryId, int transactionId, UpdateTransactionRequestDto request, int userId)
        {
            var diary = await _diaryRepository.GetByIdWithTransactionsAsync(diaryId)
                ?? throw new KeyNotFoundException($"Diary {diaryId} not found.");

            if (diary.UserId != userId)
                throw new UnauthorizedAccessException();

            var transaction = diary.Transactions.FirstOrDefault(t => t.Id == transactionId)
                ?? throw new KeyNotFoundException($"Transaction {transactionId} not found.");

            transaction.Update(
                request.Amount,
                request.CategoryId,
                request.Description,
                request.TransactionDate ?? transaction.TransactionDate,
                request.PaymentMethod);

            diary.RecalculateTotalAmount();
            await _diaryRepository.UpdateAsync(diary);
            return _transactionMapper.ToDto(transaction);
        }

        public async Task DeleteTransactionAsync(int diaryId, int transactionId, int userId)
        {
            var diary = await _diaryRepository.GetByIdWithTransactionsAsync(diaryId)
                ?? throw new KeyNotFoundException($"Diary {diaryId} not found.");

            if (diary.UserId != userId)
                throw new UnauthorizedAccessException();

            var transaction = diary.Transactions.FirstOrDefault(t => t.Id == transactionId)
                ?? throw new KeyNotFoundException($"Transaction {transactionId} not found.");

            diary.RemoveTransaction(transaction);
            await _diaryRepository.UpdateAsync(diary);
        }
    }
}
