using MyAdvisor.Client.Models.Common;
using MyAdvisor.Client.Models.Diary;
using System.Net.Http.Json;

namespace MyAdvisor.Client.Services;

public class DiaryService(HttpClient http)
{
    public async Task<List<FinancialDiaryModel>> GetAllAsync()
    {
        var res = await http.GetAsync("/api/financialdiary");
        await ThrowIfErrorAsync(res, "Failed to load diaries.");
        return await res.Content.ReadFromJsonAsync<List<FinancialDiaryModel>>() ?? [];
    }

    public async Task<FinancialDiaryModel> CreateAsync(CreateDiaryModel model)
    {
        var res = await http.PostAsJsonAsync("/api/financialdiary", new
        {
            date = model.Date.ToString("yyyy-MM-dd"),
            notes = model.Notes
        });
        await ThrowIfErrorAsync(res, "Failed to create diary.");
        return (await res.Content.ReadFromJsonAsync<FinancialDiaryModel>())!;
    }

    public async Task<FinancialDiaryModel> UpdateAsync(int id, string? notes)
    {
        var res = await http.PutAsJsonAsync($"/api/financialdiary/{id}", new { notes });
        await ThrowIfErrorAsync(res, "Failed to update diary.");
        return (await res.Content.ReadFromJsonAsync<FinancialDiaryModel>())!;
    }

    public async Task DeleteAsync(int id)
    {
        var res = await http.DeleteAsync($"/api/financialdiary/{id}");
        await ThrowIfErrorAsync(res, "Failed to delete diary.");
    }

    private static async Task ThrowIfErrorAsync(HttpResponseMessage res, string fallback)
    {
        if (res.IsSuccessStatusCode) return;
        var data = await res.Content.ReadFromJsonAsync<ErrorResponse>();
        throw new Exception(data?.Error ?? fallback);
    }
}
