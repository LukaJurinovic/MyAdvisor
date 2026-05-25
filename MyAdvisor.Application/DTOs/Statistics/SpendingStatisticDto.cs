namespace MyAdvisor.Application.DTOs.Statistics
{
    public record SpendingStatisticDto(
        int Year,
        int Month,
        decimal TotalSpent,
        decimal TotalIncome,
        decimal NetAmount,
        IReadOnlyList<CategoryBreakdownDto> CategoryBreakdown,
        IReadOnlyList<CategoryBreakdownDto> IncomeBreakdown
    );
}
