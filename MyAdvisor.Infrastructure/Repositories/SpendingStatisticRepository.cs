using Microsoft.EntityFrameworkCore;
using MyAdvisor.Application.Interfaces.Repositories;
using MyAdvisor.Domain.Entities;
using MyAdvisor.Infrastructure.Persistence;

namespace MyAdvisor.Infrastructure.Repositories
{
    public class SpendingStatisticRepository : ISpendingStatisticRepository
    {
        private readonly AppDbContext _db;

        public SpendingStatisticRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<SpendingStatistic?> GetByUserIdAndPeriodAsync(int userId, int month, int year)
            => _db.SpendingStatistics
                .Include(s => s.CategoryBreakdown)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Month == month && s.Year == year);

        public async Task<IReadOnlyList<SpendingStatistic>> GetByUserIdAndYearAsync(int userId, int year)
            => await _db.SpendingStatistics
                .Include(s => s.CategoryBreakdown)
                .Where(s => s.UserId == userId && s.Year == year)
                .ToListAsync();

        public async Task ReplaceAsync(SpendingStatistic statistic, int userId, int year, int month)
        {
            var existing = await _db.SpendingStatistics
                .Include(s => s.CategoryBreakdown)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Year == year && s.Month == month);
            if (existing is not null)
                _db.SpendingStatistics.Remove(existing);
            await _db.SpendingStatistics.AddAsync(statistic);
            await _db.SaveChangesAsync();
        }
    }
}
