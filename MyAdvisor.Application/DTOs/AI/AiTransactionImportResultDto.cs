using MyAdvisor.Application.DTOs.Transaction;

namespace MyAdvisor.Application.DTOs.AI
{
    public record AiTransactionImportResultDto(
        IReadOnlyList<TransactionDto> ImportedTransactions,
        int TotalFound,
        int SuccessfullyImported,
        int FailedCount
    );
}
