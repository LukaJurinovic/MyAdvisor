using MyAdvisor.Client.Models.Common;
using MyAdvisor.Client.Models.Transaction;
using System.Net.Http.Json;

namespace MyAdvisor.Client.Services;

public class TransactionService(HttpClient http)
{
    public async Task<List<TransactionModel>> GetByDiaryIdAsync(int diaryId)
    {
        var res = await http.GetAsync($"/api/transaction/diary/{diaryId}");
        await ThrowIfErrorAsync(res, "Failed to load transactions.");
        return await res.Content.ReadFromJsonAsync<List<TransactionModel>>() ?? [];
    }

    public async Task<TransactionModel> AddAsync(int diaryId, AddTransactionModel model)
    {
        var res = await http.PostAsJsonAsync("/api/transaction", new
        {
            diaryId,
            model.Amount,
            model.CategoryId,
            model.Description,
            model.TransactionDate,
            model.PaymentMethod
        });
        await ThrowIfErrorAsync(res, "Failed to add transaction.");
        return (await res.Content.ReadFromJsonAsync<TransactionModel>())!;
    }

    public async Task<TransactionModel> UpdateAsync(int id, UpdateTransactionModel model)
    {
        var res = await http.PutAsJsonAsync($"/api/transaction/{id}", new
        {
            model.Amount,
            model.CategoryId,
            model.Description,
            model.TransactionDate,
            model.PaymentMethod
        });
        await ThrowIfErrorAsync(res, "Failed to update transaction.");
        return (await res.Content.ReadFromJsonAsync<TransactionModel>())!;
    }

    public async Task DeleteAsync(int id)
    {
        var res = await http.DeleteAsync($"/api/transaction/{id}");
        await ThrowIfErrorAsync(res, "Failed to delete transaction.");
    }

    private static async Task ThrowIfErrorAsync(HttpResponseMessage res, string fallback)
    {
        if (res.IsSuccessStatusCode) return;
        var data = await res.Content.ReadFromJsonAsync<ErrorResponse>();
        throw new Exception(data?.Error ?? fallback);
    }
}
