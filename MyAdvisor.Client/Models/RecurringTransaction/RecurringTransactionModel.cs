namespace MyAdvisor.Client.Models.RecurringTransaction;

public record RecurringTransactionModel(
    int Id,
    int UserId,
    int CategoryId,
    decimal Amount,
    string Frequency,
    string? PaymentMethod,
    DateTime? NextDueDate,
    string? Description
);
