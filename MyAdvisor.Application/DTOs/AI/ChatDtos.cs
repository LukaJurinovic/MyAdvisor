namespace MyAdvisor.Application.DTOs.AI
{
    public record ChatMessageDto(string Role, string Content);

    public record ChatRequestDto(
        List<ChatMessageDto> History,
        string Message,
        bool IncludeFinancialContext
    );

    public record ChatResponseDto(string Reply);
}
