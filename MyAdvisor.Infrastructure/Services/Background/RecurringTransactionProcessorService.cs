using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyAdvisor.Application.DTOs.RecurringTransaction;
using MyAdvisor.Application.DTOs.Transaction;
using MyAdvisor.Application.Interfaces.Services.Domain;
using MyAdvisor.Domain.Enums;

namespace MyAdvisor.Infrastructure.Services.Background
{
    public class RecurringTransactionProcessorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RecurringTransactionProcessorService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromHours(1);
        private const int MaxOccurrencesPerRun = 365;

        public RecurringTransactionProcessorService(
            IServiceScopeFactory scopeFactory,
            ILogger<RecurringTransactionProcessorService> logger)
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
                var recurringService = scope.ServiceProvider.GetRequiredService<IRecurringTransactionService>();
                var diaryService = scope.ServiceProvider.GetRequiredService<IFinancialDiaryService>();

                var due = await recurringService.GetAllDueAsync(DateTime.UtcNow);
                if (!due.Any()) return;

                _logger.LogInformation("Processing {Count} due recurring transaction(s).", due.Count);

                foreach (var recurring in due)
                {
                    try
                    {
                        await ProcessOneAsync(recurring, recurringService, diaryService);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process recurring transaction {Id}.", recurring.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing recurring transactions.");
            }
        }

        private async Task ProcessOneAsync(
            RecurringTransactionDto recurring,
            IRecurringTransactionService recurringService,
            IFinancialDiaryService diaryService)
        {
            var dueDate = recurring.NextDueDate!.Value;
            var today = DateTime.UtcNow.Date;
            var frequency = Enum.Parse<Frequency>(recurring.Frequency);
            var paymentMethod = Enum.TryParse<PaymentMethod>(recurring.PaymentMethod, out var pm) ? pm : (PaymentMethod?)null;
            var iterations = 0;

            while (dueDate.Date <= today && iterations < MaxOccurrencesPerRun)
            {
                iterations++;
                var nextDueDate = Advance(dueDate, frequency);

                var diaryId = await diaryService.EnsureDiaryExistsAsync(recurring.UserId, dueDate.Date);

                await diaryService.AddTransactionAsync(
                    new AddTransactionRequestDto(
                        diaryId,
                        recurring.Amount,
                        recurring.CategoryId,
                        recurring.Description,
                        dueDate,
                        paymentMethod),
                    recurring.UserId);

                await recurringService.AdvanceDueDateAsync(recurring.Id, nextDueDate);
                dueDate = nextDueDate;
            }

            if (iterations > 0)
                _logger.LogInformation(
                    "Created {Count} transaction(s) for recurring {Id} ('{Desc}'). Next due: {Next:yyyy-MM-dd}.",
                    iterations, recurring.Id, recurring.Description, dueDate);
        }

        private static DateTime Advance(DateTime date, Frequency frequency) => frequency switch
        {
            Frequency.Daily => date.AddDays(1),
            Frequency.Weekly => date.AddDays(7),
            Frequency.Monthly => date.AddMonths(1),
            Frequency.Yearly => date.AddYears(1),
            _ => date.AddMonths(1)
        };
    }
}
