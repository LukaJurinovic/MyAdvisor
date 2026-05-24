namespace MyAdvisor.Application.DTOs.Statistics
{
    public record CategoryBreakdownDto(
        int? CategoryId,
        string CategoryName,
        decimal TotalAmount,
        decimal Percentage
    );
}
