using MyAdvisor.Application.DTOs.AI;

namespace MyAdvisor.Application.Interfaces.Services.App
{
    public interface IFinancialChatService
    {
        Task<ChatResponseDto> ChatAsync(int userId, ChatRequestDto request);
        Task<ChatResponseDto> SummarizeImageAsync(byte[] imageData, string mimeType);
    }
}
