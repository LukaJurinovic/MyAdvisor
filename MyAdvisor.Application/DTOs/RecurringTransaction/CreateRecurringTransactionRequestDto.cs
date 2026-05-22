using System.ComponentModel.DataAnnotations;
using MyAdvisor.Domain.Enums;

namespace MyAdvisor.Application.DTOs.RecurringTransaction
{
    public record CreateRecurringTransactionRequestDto(
        [Required] int CategoryId,
        [Required] decimal Amount,
        [Required] Frequency? Frequency,
        PaymentMethod? PaymentMethod,
        DateTime? NextDueDate,
        string? Description
    );
}
