using MyAdvisor.Client.Models.Common;
using MyAdvisor.Client.Models.Statistics;
using System.Net.Http.Json;

namespace MyAdvisor.Client.Services;

public class StatisticsService(HttpClient http)
{
    public async Task<SpendingStatisticModel> GetMonthlyAsync(int year, int month)
    {
        var res = await http.GetAsync($"/api/statistics/monthly?year={year}&month={month}");
        await ThrowIfErrorAsync(res, "Failed to load spending statistics.");
        return (await res.Content.ReadFromJsonAsync<SpendingStatisticModel>())!;
    }

    private static async Task ThrowIfErrorAsync(HttpResponseMessage res, string fallback)
    {
        if (res.IsSuccessStatusCode) return;
        var data = await res.Content.ReadFromJsonAsync<ErrorResponse>();
        throw new Exception(data?.Error ?? fallback);
    }
}
