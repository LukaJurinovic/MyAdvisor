using System.ComponentModel.DataAnnotations;

namespace MyAdvisor.Client.Models.RecurringTransaction;

public class CreateRecurringTransactionModel
{
    [Required]
    public int? CategoryId { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    public string? Frequency { get; set; }

    public string? PaymentMethod { get; set; }

    public DateTime? NextDueDate { get; set; }

    public string? Description { get; set; }
}
