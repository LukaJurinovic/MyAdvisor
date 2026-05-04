using Microsoft.EntityFrameworkCore;
using MyAdvisor.Application.Interfaces.Repositories;
using MyAdvisor.Domain.Entities;
using MyAdvisor.Infrastructure.Persistence;

namespace MyAdvisor.Infrastructure.Repositories
{
    public class FinancialDiaryRepository : IFinancialDiaryRepository
    {
        private readonly AppDbContext _db;

        public FinancialDiaryRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<FinancialDiary?> GetByIdAsync(int id)
            => _db.FinancialDiaries.FirstOrDefaultAsync(d => d.Id == id);

        public Task<FinancialDiary?> GetByIdWithTransactionsAsync(int id)
            => _db.FinancialDiaries
                .Include(d => d.Transactions)
                .FirstOrDefaultAsync(d => d.Id == id);

        public async Task<IReadOnlyList<FinancialDiary>> GetByUserIdAsync(int userId)
            => await _db.FinancialDiaries.Where(d => d.UserId == userId).ToListAsync();

        public async Task<IReadOnlyList<FinancialDiary>> GetByUserIdWithTransactionsAsync(int userId)
            => await _db.FinancialDiaries
                .Where(d => d.UserId == userId)
                .Include(d => d.Transactions)
                .ToListAsync();

        public Task<FinancialDiary?> GetByUserIdAndDateAsync(int userId, DateTime date)
            => _db.FinancialDiaries.FirstOrDefaultAsync(d => d.UserId == userId && d.Date == date.Date);

        public async Task AddAsync(FinancialDiary diary)
        {
            await _db.FinancialDiaries.AddAsync(diary);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(FinancialDiary diary)
        {
            _db.FinancialDiaries.Update(diary);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var diary = await _db.FinancialDiaries.FindAsync(id);
            if (diary is not null)
            {
                _db.FinancialDiaries.Remove(diary);
                await _db.SaveChangesAsync();
            }
        }
    }
}
