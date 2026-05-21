using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyAdvisor.Application.Interfaces.Repositories;
using MyAdvisor.Domain.Entities;
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
                var recurringRepo = scope.ServiceProvider.GetRequiredService<IRecurringTransactionRepository>();
                var diaryRepo = scope.ServiceProvider.GetRequiredService<IFinancialDiaryRepository>();

                var due = await recurringRepo.GetAllDueAsync(DateTime.UtcNow);
                if (!due.Any()) return;

                _logger.LogInformation("Processing {Count} due recurring transaction(s).", due.Count);

                foreach (var recurring in due)
                {
                    await ProcessOneAsync(recurring, recurringRepo, diaryRepo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing recurring transactions.");
            }
        }

        private async Task ProcessOneAsync(
            RecurringTransaction recurring,
            IRecurringTransactionRepository recurringRepo,
            IFinancialDiaryRepository diaryRepo)
        {
            var dueDate = recurring.NextDueDate!.Value;
            var today = DateTime.UtcNow.Date;
            var iterations = 0;

            while (dueDate.Date <= today && iterations < MaxOccurrencesPerRun)
            {
                iterations++;

                var diary = await diaryRepo.GetByUserIdAndDateWithTransactionsAsync(recurring.UserId, dueDate.Date);
                if (diary is null)
                {
                    diary = new FinancialDiary(recurring.UserId, dueDate.Date);
                    await diaryRepo.AddAsync(diary);
                    // reload to get the assigned Id
                    diary = (await diaryRepo.GetByUserIdAndDateWithTransactionsAsync(recurring.UserId, dueDate.Date))!;
                }

                var transaction = new Transaction(
                    diary.Id,
                    recurring.Amount,
                    recurring.CategoryId,
                    recurring.Description,
                    dueDate,
                    recurring.PaymentMethod);

                diary.AddTransaction(transaction);
                await diaryRepo.UpdateAsync(diary);

                dueDate = Advance(dueDate, recurring.Frequency);
            }

            if (iterations > 0)
            {
                recurring.AdvanceDueDate(dueDate);
                await recurringRepo.UpdateAsync(recurring);
                _logger.LogInformation(
                    "Created {Count} transaction(s) for recurring {Id} ('{Desc}'). Next due: {Next:yyyy-MM-dd}.",
                    iterations, recurring.Id, recurring.Description, dueDate);
            }
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
