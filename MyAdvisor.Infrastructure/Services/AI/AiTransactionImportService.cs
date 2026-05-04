using System.Text.Json;
using Microsoft.Extensions.Logging;
using MyAdvisor.Application.DTOs.AI;
using MyAdvisor.Application.DTOs.Transaction;
using MyAdvisor.Application.Interfaces.Contracts;
using MyAdvisor.Application.Interfaces.Services.AI;
using MyAdvisor.Application.Interfaces.Services.App;
using MyAdvisor.Application.Interfaces.Services.Domain;
using MyAdvisor.Domain.Enums;

namespace MyAdvisor.Infrastructure.Services.AI
{
    public class AiTransactionImportService : IAiTransactionImportService
    {
        private readonly IGeminiService _gemini;
        private readonly IFinancialDiaryService _diaryService;
        private readonly ICategoryService _categoryService;
        private readonly ITransactionAiLogService _aiLogService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AiTransactionImportService> _logger;

        public AiTransactionImportService(
            IGeminiService gemini,
            IFinancialDiaryService diaryService,
            ICategoryService categoryService,
            ITransactionAiLogService aiLogService,
            IUnitOfWork unitOfWork,
            ILogger<AiTransactionImportService> logger)
        {
            _gemini = gemini;
            _diaryService = diaryService;
            _categoryService = categoryService;
            _aiLogService = aiLogService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<AiImportPreviewDto> PreviewFromImageAsync(int diaryId, int userId, byte[] imageData, string mimeType)
        {
            var diary = await _diaryService.GetByIdAsync(diaryId)
                ?? throw new KeyNotFoundException($"Diary {diaryId} not found.");

            if (diary.UserId != userId)
                throw new UnauthorizedAccessException();

            var categories = await _categoryService.GetAllAsync();
            var categoryNames = string.Join(", ", categories.Select(c => c.Name));

            var rawResponse = await _gemini.AnalyzeImageAsync(imageData, mimeType, BuildPrompt(categoryNames));
            var parsedItems = ParseGeminiResponse(rawResponse);

            var existingNames = categories.Select(c => c.Name.ToLowerInvariant()).ToHashSet();
            var newCategories = parsedItems
                .Where(i => !string.IsNullOrWhiteSpace(i.CategoryName) && !existingNames.Contains(i.CategoryName!.ToLowerInvariant()))
                .Select(i => i.CategoryName!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var pending = parsedItems.Select(i => new PendingTransactionDto(
                Amount: i.Amount,
                Description: i.Description,
                CategoryName: i.CategoryName,
                IsNewCategory: !string.IsNullOrWhiteSpace(i.CategoryName) && !existingNames.Contains(i.CategoryName.ToLowerInvariant()),
                PaymentMethod: i.PaymentMethod,
                TransactionDate: DateTime.TryParse(i.TransactionDate, out var d) ? d : null,
                Confidence: i.Confidence
            )).ToList();

            return new AiImportPreviewDto(pending, newCategories);
        }

        public async Task<AiTransactionImportResultDto> ConfirmImportAsync(int userId, AiConfirmImportRequestDto request)
        {
            var imported = new List<TransactionDto>();
            var failed = 0;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                foreach (var newCatName in request.ApprovedNewCategories)
                    await _categoryService.EnsureCreatedAsync(newCatName);

                var categories = await _categoryService.GetAllAsync();
                var catLookup = categories.ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase);

                foreach (var item in request.ApprovedTransactions)
                {
                    try
                    {
                        catLookup.TryGetValue(item.CategoryName ?? string.Empty, out var matchedCategory);

                        var addRequest = new AddTransactionRequestDto(
                            DiaryId: request.DiaryId,
                            Amount: item.Amount,
                            CategoryId: matchedCategory?.Id,
                            Description: item.Description,
                            TransactionDate: item.TransactionDate,
                            PaymentMethod: ParsePaymentMethod(item.PaymentMethod)
                        );

                        var transactionDto = await _diaryService.AddTransactionAsync(addRequest, userId);
                        imported.Add(transactionDto);

                        var confidence = matchedCategory is not null
                            ? Math.Clamp(item.Confidence, 0.5m, 1.0m)
                            : Math.Clamp(item.Confidence * 0.6m, 0m, 1m);

                        await _aiLogService.AddAsync(
                            transactionId: transactionDto.Id,
                            rawOcrText: item.Description,
                            aiCategoryId: matchedCategory?.Id,
                            confidence: confidence);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Skipped transaction item due to processing error: {Description}", item.Description);
                        failed++;
                    }
                }
            });

            return new AiTransactionImportResultDto(
                ImportedTransactions: imported,
                TotalFound: request.ApprovedTransactions.Count,
                SuccessfullyImported: imported.Count,
                FailedCount: failed
            );
        }

        private static string BuildPrompt(string categoryNames) => $$"""
            You are a precise financial transaction extractor.
            Analyze the image (bank statement, receipt, expense list, or similar document) and extract individual purchased items as separate transactions.

            Respond ONLY with a raw JSON array. No explanation, no markdown, no code fences — just the array itself.

            Each object must have EXACTLY these fields:
            - "amount":          number — the actual total price paid for that line item (negative for expenses/payments, positive for income/deposits)
            - "description":     string or null
            - "categoryName":    string — see rules below
            - "paymentMethod":   string — one of: Cash, Card, Transfer, Other
            - "transactionDate": string in "yyyy-MM-dd" format (use today's date if not visible)
            - "confidence":      number between 0.0 and 1.0

            SKIP RULES (critical — do NOT create an entry for any of these lines):
            - Receipt totals / subtotals: lines labelled UKUPNO, SVEUKUPNO, TOTAL, SUBTOTAL, MEĐUZBROJ, or any "TOTAL" variant
            - Tax lines: PDV, VAT, porez, or any line that represents a tax amount
            - Payment confirmation lines: lines starting with PLAĆENO, PLAĆANJE, PAID, PAYMENT, or listing a tender amount (e.g. "PLAĆENO: Gotovina 110,55")
            - Summary discount lines: "Ukupan popust", "Ukupni popust", "Ukupno popust" — these are totals of discounts already counted in item prices
            - Any line that is clearly a receipt footer, store info, cashier info, or barcode

            AMOUNT RULES (critical):
            - For weight-based items (e.g. "Banana 0.5 kg x 3.50 €/kg = 1.75 €"), use the final line total (1.75), NOT the unit price per kg (3.50)
            - Individual discount lines (POPUST, RABAT, DISCOUNT) that appear right after a specific item are valid entries — use a POSITIVE amount (e.g. +2.30) because they reduce what you paid, not add to it

            PAYMENT METHOD RULES (critical):
            - Look for a line starting with PLAĆENO or similar near the bottom of the receipt — it tells you how the entire purchase was paid
            - "Gotovina" or "GOTOVINA" = Cash → set paymentMethod to "Cash" for ALL items on that receipt
            - "Kartica" or "KARTICA" or "KREDITNA" = Card → set paymentMethod to "Card" for ALL items
            - "Transakcija" or "TRANSAKCIJA" = Transfer → set paymentMethod to "Transfer" for ALL items
            - If no payment line is found, default to "Cash"
            - Apply the same paymentMethod to every item — do not use "Other" unless the method is truly ambiguous

            CATEGORY RULES (important):
            - First try to match from this existing list: {{categoryNames}}
            - If a good match exists, use that exact name
            - If NO good match exists, invent a short descriptive category name (e.g. "Pet Care", "Medical", "Subscriptions", "Home Repair")
            - NEVER use "Other" — always use a specific descriptive name

            Example (item + discount on a receipt paid by cash):
            [
              {"amount":-1.75,"description":"Banana 0.5 kg","categoryName":"Groceries","paymentMethod":"Cash","transactionDate":"2025-03-01","confidence":0.95},
              {"amount":0.35,"description":"POPUST","categoryName":"Groceries","paymentMethod":"Cash","transactionDate":"2025-03-01","confidence":0.95}
            ]
            Note: the discount entry has a POSITIVE amount (0.35, not -0.35) even if the receipt prints it with a minus sign.

            If no transactions are found, return: []
            """;

        private static readonly HashSet<string> _discountKeywords =
            ["popust", "rabat", "discount", "sniženje", "akcija"];

        private static List<ParsedTransaction> ParseGeminiResponse(string raw)
        {
            List<ParsedTransaction> items;
            try
            {
                items = JsonSerializer.Deserialize<List<ParsedTransaction>>(
                    raw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? [];
            }
            catch { return []; }

            foreach (var item in items)
            {
                var desc = item.Description?.ToLowerInvariant() ?? "";
                if (_discountKeywords.Any(k => desc.Contains(k)) && item.Amount < 0)
                    item.Amount = -item.Amount;
            }

            return items;
        }

        private static PaymentMethod? ParsePaymentMethod(string? raw) =>
            raw?.Trim().ToLowerInvariant() switch
            {
                "cash" => PaymentMethod.Cash,
                "card" => PaymentMethod.Card,
                "transfer" => PaymentMethod.Transfer,
                _ => PaymentMethod.Other
            };

        private sealed class ParsedTransaction
        {
            public decimal Amount { get; set; }
            public string? Description { get; set; }
            public string? CategoryName { get; set; }
            public string? PaymentMethod { get; set; }
            public string? TransactionDate { get; set; }
            public decimal Confidence { get; set; }
        }
    }
}
