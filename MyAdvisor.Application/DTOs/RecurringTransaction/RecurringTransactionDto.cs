namespace MyAdvisor.Application.DTOs.RecurringTransaction
{
    public record RecurringTransactionDto(
        int Id,
        int UserId,
        int CategoryId,
        decimal Amount,
        string Frequency,
        string? PaymentMethod,
        DateTime? NextDueDate,
        string? Description
    );
}
