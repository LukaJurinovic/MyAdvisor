namespace MyAdvisor.Domain.Entities
{
    public class SpendingStatistic
    {
        private readonly List<CategoryStatistic> _categoryBreakdown = new();

        public int Id { get; private set; }
        public int UserId { get; private set; }
        public int Month { get; private set; }
        public int Year { get; private set; }
        public decimal TotalSpent { get; private set; }
        public decimal TotalIncome { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public User? User { get; private set; }
        public IReadOnlyCollection<CategoryStatistic> CategoryBreakdown => _categoryBreakdown.AsReadOnly();

        private SpendingStatistic() { }

        public SpendingStatistic(int userId, int month, int year, decimal totalSpent, decimal totalIncome)
        {
            if (userId <= 0) throw new ArgumentException("Invalid userId.", nameof(userId));
            if (month < 1 || month > 12) throw new ArgumentException("Month must be between 1 and 12.", nameof(month));
            if (year < 2000) throw new ArgumentException("Invalid year.", nameof(year));

            UserId = userId;
            Month = month;
            Year = year;
            TotalSpent = totalSpent;
            TotalIncome = totalIncome;
            CreatedAt = DateTime.UtcNow;
        }

        public void AddCategoryStatistic(CategoryStatistic categoryStatistic)
        {
            ArgumentNullException.ThrowIfNull(categoryStatistic);
            _categoryBreakdown.Add(categoryStatistic);
        }
    }
}
