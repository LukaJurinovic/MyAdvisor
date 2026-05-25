namespace MyAdvisor.Domain.Entities
{
    public class CategoryStatistic
    {
        public int Id { get; private set; }
        public int StatisticsId { get; private set; }
        public int? CategoryId { get; private set; }
        public string CategoryName { get; private set; } = string.Empty;
        public decimal TotalAmount { get; private set; }
        public decimal Percentage { get; private set; }
        public bool IsIncome { get; private set; }
        public SpendingStatistic? Statistics { get; private set; }
        public Category? Category { get; private set; }

        private CategoryStatistic() { }

        public CategoryStatistic(SpendingStatistic statistics, int? categoryId, string categoryName,
            decimal totalAmount, decimal percentage, bool isIncome)
        {
            ArgumentNullException.ThrowIfNull(statistics);
            if (totalAmount < 0)
                throw new ArgumentException("TotalAmount cannot be negative.", nameof(totalAmount));

            Statistics = statistics;
            CategoryId = categoryId;
            CategoryName = categoryName ?? string.Empty;
            TotalAmount = totalAmount;
            Percentage = percentage;
            IsIncome = isIncome;
        }
    }
}
