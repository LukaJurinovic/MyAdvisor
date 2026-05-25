namespace MyAdvisor.Client.Models.Statistics;

public record SpendingStatisticModel(
    int Year,
    int Month,
    decimal TotalSpent,
    decimal TotalIncome,
    decimal NetAmount,
    List<CategoryBreakdownModel> CategoryBreakdown,
    List<CategoryBreakdownModel> IncomeBreakdown
);
