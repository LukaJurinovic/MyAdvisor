namespace MyAdvisor.Application.Interfaces.Services.Domain
{
    public interface ITransactionAiLogService
    {
        Task AddAsync(int transactionId, string? rawOcrText, int? aiCategoryId, decimal? confidence);
    }
}
