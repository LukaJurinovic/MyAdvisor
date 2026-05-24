using System.ComponentModel.DataAnnotations;
using MyAdvisor.Domain.Enums;

namespace MyAdvisor.Application.DTOs.Transaction
{
    public record AddTransactionRequestDto(
        [Required] int DiaryId,
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        [Required] decimal Amount,
        int? CategoryId,
        string? Description,
        DateTime? TransactionDate,
        [Required] PaymentMethod? PaymentMethod
    );
}
