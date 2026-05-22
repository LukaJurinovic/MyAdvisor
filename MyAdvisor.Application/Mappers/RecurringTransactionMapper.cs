using MyAdvisor.Application.DTOs.RecurringTransaction;
using MyAdvisor.Domain.Entities;

namespace MyAdvisor.Application.Mappers
{
    public class RecurringTransactionMapper
    {
        public RecurringTransactionDto ToDto(RecurringTransaction entity) =>
            new(
                entity.Id,
                entity.UserId,
                entity.CategoryId,
                entity.Amount,
                entity.Frequency.ToString(),
                entity.PaymentMethod?.ToString(),
                entity.NextDueDate,
                entity.Description
            );
    }
}
