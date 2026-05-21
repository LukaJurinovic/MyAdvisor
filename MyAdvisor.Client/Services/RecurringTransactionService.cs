using MyAdvisor.Client.Models.Common;
using MyAdvisor.Client.Models.RecurringTransaction;
using System.Net.Http.Json;

namespace MyAdvisor.Client.Services;

public class RecurringTransactionService(HttpClient http)
{
    public async Task<List<RecurringTransactionModel>> GetAllAsync()
    {
        var res = await http.GetAsync("/api/recurringtransaction");
        await ThrowIfErrorAsync(res, "Failed to load recurring transactions.");
        return await res.Content.ReadFromJsonAsync<List<RecurringTransactionModel>>() ?? [];
    }

    public async Task<RecurringTransactionModel> GetByIdAsync(int id)
    {
        var res = await http.GetAsync($"/api/recurringtransaction/{id}");
        await ThrowIfErrorAsync(res, "Failed to load recurring transaction.");
        return (await res.Content.ReadFromJsonAsync<RecurringTransactionModel>())!;
    }

    public async Task<RecurringTransactionModel> CreateAsync(CreateRecurringTransactionModel model)
    {
        var res = await http.PostAsJsonAsync("/api/recurringtransaction", new
        {
            model.CategoryId,
            model.Amount,
            model.Frequency,
            model.PaymentMethod,
            model.NextDueDate,
            model.Description
        });
        await ThrowIfErrorAsync(res, "Failed to create recurring transaction.");
        return (await res.Content.ReadFromJsonAsync<RecurringTransactionModel>())!;
    }

    public async Task<RecurringTransactionModel> UpdateAsync(int id, UpdateRecurringTransactionModel model)
    {
        var res = await http.PutAsJsonAsync($"/api/recurringtransaction/{id}", new
        {
            model.CategoryId,
            model.Amount,
            model.Frequency,
            model.PaymentMethod,
            model.NextDueDate,
            model.Description
        });
        await ThrowIfErrorAsync(res, "Failed to update recurring transaction.");
        return (await res.Content.ReadFromJsonAsync<RecurringTransactionModel>())!;
    }

    public async Task DeleteAsync(int id)
    {
        var res = await http.DeleteAsync($"/api/recurringtransaction/{id}");
        await ThrowIfErrorAsync(res, "Failed to delete recurring transaction.");
    }

    private static async Task ThrowIfErrorAsync(HttpResponseMessage res, string fallback)
    {
        if (res.IsSuccessStatusCode) return;
        var data = await res.Content.ReadFromJsonAsync<ErrorResponse>();
        throw new Exception(data?.Error ?? fallback);
    }
}
