using MyAdvisor.Domain.Entities;

namespace MyAdvisor.Application.Interfaces.Repositories
{
    public interface ITransactionAiLogRepository
    {
        Task AddAsync(TransactionAiLog log);
    }
}
