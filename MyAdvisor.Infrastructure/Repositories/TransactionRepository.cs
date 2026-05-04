using Microsoft.EntityFrameworkCore;
using MyAdvisor.Application.Interfaces.Repositories;
using MyAdvisor.Domain.Entities;
using MyAdvisor.Infrastructure.Persistence;

namespace MyAdvisor.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _db;

        public TransactionRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<Transaction?> GetByIdAsync(int id)
            => _db.Transactions.FirstOrDefaultAsync(t => t.Id == id);

        public async Task<IReadOnlyList<Transaction>> GetByDiaryIdAsync(int diaryId)
            => await _db.Transactions.Where(t => t.DiaryId == diaryId).ToListAsync();
    }
}
