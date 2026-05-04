using MyAdvisor.Application.DTOs.AI;

namespace MyAdvisor.Application.Interfaces.Services.App
{
    public interface IAiTransactionImportService
    {
        Task<AiImportPreviewDto> PreviewFromImageAsync(int diaryId, int userId, byte[] imageData, string mimeType);
        Task<AiTransactionImportResultDto> ConfirmImportAsync(int userId, AiConfirmImportRequestDto request);
    }
}
