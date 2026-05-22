using Microsoft.EntityFrameworkCore;
using MyAdvisor.Application.Interfaces.Repositories;
using MyAdvisor.Domain.Entities;
using MyAdvisor.Infrastructure.Persistence;

namespace MyAdvisor.Infrastructure.Repositories
{
    public class RecurringTransactionRepository : IRecurringTransactionRepository
    {
        private readonly AppDbContext _db;

        public RecurringTransactionRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<RecurringTransaction?> GetByIdAsync(int id)
            => _db.RecurringTransactions.FirstOrDefaultAsync(r => r.Id == id);

        public async Task<IReadOnlyList<RecurringTransaction>> GetByUserIdAsync(int userId)
            => await _db.RecurringTransactions.Where(r => r.UserId == userId).ToListAsync();

        public async Task<IReadOnlyList<RecurringTransaction>> GetAllDueAsync(DateTime asOf)
            => await _db.RecurringTransactions
                .Where(r => r.NextDueDate.HasValue && r.NextDueDate.Value.Date <= asOf.Date)
                .ToListAsync();

        public async Task AddAsync(RecurringTransaction recurringTransaction)
        {
            await _db.RecurringTransactions.AddAsync(recurringTransaction);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(RecurringTransaction recurringTransaction)
        {
            _db.RecurringTransactions.Update(recurringTransaction);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var recurringTransaction = await _db.RecurringTransactions.FindAsync(id);
            if (recurringTransaction is not null)
            {
                _db.RecurringTransactions.Remove(recurringTransaction);
                await _db.SaveChangesAsync();
            }
        }
    }
}
