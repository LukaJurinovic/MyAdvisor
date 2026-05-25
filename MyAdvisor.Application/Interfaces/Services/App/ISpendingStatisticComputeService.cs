namespace MyAdvisor.Application.Interfaces.Services.App
{
    public interface ISpendingStatisticComputeService
    {
        Task RecomputeForUserAsync(int userId);
    }
}
