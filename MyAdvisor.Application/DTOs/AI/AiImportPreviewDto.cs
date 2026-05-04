namespace MyAdvisor.Application.DTOs.AI
{
    public record AiImportPreviewDto(
        List<PendingTransactionDto> PendingTransactions,
        List<string> NewCategorySuggestions
    );

    public record PendingTransactionDto(
        decimal Amount,
        string? Description,
        string? CategoryName,
        bool IsNewCategory,
        string? PaymentMethod,
        DateTime? TransactionDate,
        decimal Confidence
    );
}
