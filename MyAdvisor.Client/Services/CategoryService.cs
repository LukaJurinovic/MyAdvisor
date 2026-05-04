using MyAdvisor.Client.Models.Category;
using MyAdvisor.Client.Models.Common;
using System.Net.Http.Json;

namespace MyAdvisor.Client.Services;

public class CategoryService(HttpClient http)
{
    public async Task<List<CategoryModel>> GetAllAsync()
    {
        var res = await http.GetAsync("/api/category");
        await ThrowIfErrorAsync(res, "Failed to load categories.");
        return await res.Content.ReadFromJsonAsync<List<CategoryModel>>() ?? [];
    }

    public async Task<CategoryModel> CreateAsync(string name, int? parentCategoryId = null)
    {
        var res = await http.PostAsJsonAsync("/api/category", new { name, parentCategoryId });
        await ThrowIfErrorAsync(res, "Failed to create category.");
        return (await res.Content.ReadFromJsonAsync<CategoryModel>())!;
    }

    private static async Task ThrowIfErrorAsync(HttpResponseMessage res, string fallback)
    {
        if (res.IsSuccessStatusCode) return;
        var data = await res.Content.ReadFromJsonAsync<ErrorResponse>();
        throw new Exception(data?.Error ?? fallback);
    }
}
