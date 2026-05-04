using MyAdvisor.Client.Models.AI;
using MyAdvisor.Client.Models.Common;
using System.Net.Http.Json;

namespace MyAdvisor.Client.Services
{
    public class AiImportService(HttpClient http)
    {
        public async Task<AiImportPreviewModel> PreviewAsync(int diaryId, Stream imageStream, string fileName, string contentType)
        {
            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(imageStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(streamContent, "image", fileName);

            var res = await http.PostAsync($"/api/aitransaction/preview?diaryId={diaryId}", content);
            await ThrowIfErrorAsync(res, "AI preview failed.");
            return (await res.Content.ReadFromJsonAsync<AiImportPreviewModel>())!;
        }

        public async Task<object> ConfirmAsync(AiConfirmImportRequest request)
        {
            var res = await http.PostAsJsonAsync("/api/aitransaction/confirm", new
            {
                diaryId = request.DiaryId,
                approvedTransactions = request.ApprovedTransactions.Select(t => new
                {
                    t.Amount, t.Description, t.CategoryName, t.IsNewCategory,
                    t.PaymentMethod, t.TransactionDate, t.Confidence
                }),
                approvedNewCategories = request.ApprovedNewCategories
            });
            await ThrowIfErrorAsync(res, "AI import confirmation failed.");
            return (await res.Content.ReadFromJsonAsync<object>())!;
        }

        private static async Task ThrowIfErrorAsync(HttpResponseMessage res, string fallback)
        {
            if (res.IsSuccessStatusCode) return;
            var data = await res.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new Exception(data?.Error ?? fallback);
        }
    }
}
