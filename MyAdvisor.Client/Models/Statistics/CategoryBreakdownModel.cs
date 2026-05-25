namespace MyAdvisor.Client.Models.Statistics;

public record CategoryBreakdownModel(
    int? CategoryId,
    string CategoryName,
    decimal TotalAmount,
    decimal Percentage
);
