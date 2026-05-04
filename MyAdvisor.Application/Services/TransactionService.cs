using MyAdvisor.Application.DTOs.Transaction;
using MyAdvisor.Application.Interfaces.Repositories;
using MyAdvisor.Application.Interfaces.Services.Domain;
using MyAdvisor.Application.Mappers;

namespace MyAdvisor.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly TransactionMapper _mapper;

        public TransactionService(
            ITransactionRepository transactionRepository,
            TransactionMapper mapper)
        {
            _transactionRepository = transactionRepository;
            _mapper = mapper;
        }

        public async Task<TransactionDto> GetByIdAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Transaction {id} not found.");

            return _mapper.ToDto(transaction);
        }

        public async Task<IReadOnlyList<TransactionDto>> GetByDiaryIdAsync(int diaryId)
        {
            var transactions = await _transactionRepository.GetByDiaryIdAsync(diaryId);
            return transactions.Select(_mapper.ToDto).ToList();
        }
    }
}
