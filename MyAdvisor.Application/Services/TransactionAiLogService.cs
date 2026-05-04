using MyAdvisor.Application.Interfaces.Repositories;
using MyAdvisor.Application.Interfaces.Services.Domain;
using MyAdvisor.Domain.Entities;

namespace MyAdvisor.Application.Services
{
    public class TransactionAiLogService : ITransactionAiLogService
    {
        private readonly ITransactionAiLogRepository _repository;

        public TransactionAiLogService(ITransactionAiLogRepository repository)
        {
            _repository = repository;
        }

        public async Task AddAsync(int transactionId, string? rawOcrText, int? aiCategoryId, decimal? confidence)
        {
            var log = new TransactionAiLog(transactionId, rawOcrText, aiCategoryId, confidence);
            await _repository.AddAsync(log);
        }
    }
}
