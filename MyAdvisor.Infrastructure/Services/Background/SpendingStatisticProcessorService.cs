using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyAdvisor.Application.Interfaces.Services.App;
using MyAdvisor.Application.Interfaces.Services.Domain;

namespace MyAdvisor.Infrastructure.Services.Background
{
    public class SpendingStatisticProcessorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SpendingStatisticProcessorService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

        public SpendingStatisticProcessorService(
            IServiceScopeFactory scopeFactory,
            ILogger<SpendingStatisticProcessorService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ProcessAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(Interval, stoppingToken);
                await ProcessAsync();
            }
        }

        private async Task ProcessAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var diaryService = scope.ServiceProvider.GetRequiredService<IFinancialDiaryService>();
                var computeService = scope.ServiceProvider.GetRequiredService<ISpendingStatisticComputeService>();

                var userIds = await diaryService.GetDistinctUserIdsAsync();
                if (userIds.Count == 0) return;

                _logger.LogInformation("Recomputing spending statistics for {Count} user(s).", userIds.Count);

                foreach (var userId in userIds)
                    await computeService.RecomputeForUserAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing spending statistics.");
            }
        }
    }
}
