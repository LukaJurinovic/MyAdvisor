using MyAdvisor.Application.DTOs.RecurringTransaction;

namespace MyAdvisor.Application.Interfaces.Services.Domain
{
    public interface IRecurringTransactionService
    {
        Task<RecurringTransactionDto> GetByIdAsync(int id, int userId);
        Task<IReadOnlyList<RecurringTransactionDto>> GetByUserIdAsync(int userId);
        Task<IReadOnlyList<RecurringTransactionDto>> GetAllDueAsync(DateTime asOf);
        Task<RecurringTransactionDto> CreateAsync(CreateRecurringTransactionRequestDto request, int userId);
        Task<RecurringTransactionDto> UpdateAsync(int id, UpdateRecurringTransactionRequestDto request, int userId);
        Task DeleteAsync(int id, int userId);
        Task AdvanceDueDateAsync(int id, DateTime newDueDate);
    }
}
