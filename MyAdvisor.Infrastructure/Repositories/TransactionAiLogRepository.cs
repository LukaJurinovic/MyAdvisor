using Microsoft.EntityFrameworkCore;
using MyAdvisor.Application.Interfaces.Repositories;
using MyAdvisor.Domain.Entities;
using MyAdvisor.Infrastructure.Persistence;

namespace MyAdvisor.Infrastructure.Repositories
{
    public class TransactionAiLogRepository : ITransactionAiLogRepository
    {
        private readonly AppDbContext _db;

        public TransactionAiLogRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(TransactionAiLog log)
        {
            await _db.TransactionAiLogs.AddAsync(log);
            await _db.SaveChangesAsync();
        }
    }
}
