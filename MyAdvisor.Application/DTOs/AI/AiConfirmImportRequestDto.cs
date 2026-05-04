namespace MyAdvisor.Application.DTOs.AI
{
    public record AiConfirmImportRequestDto(
        int DiaryId,
        List<PendingTransactionDto> ApprovedTransactions,
        List<string> ApprovedNewCategories
    );
}
