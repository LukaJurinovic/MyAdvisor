namespace MyAdvisor.Client.Models.AI
{
    public class AiImportPreviewModel
    {
        public List<PendingTransactionModel> PendingTransactions { get; set; } = [];
        public List<string> NewCategorySuggestions { get; set; } = [];
    }

    public class PendingTransactionModel
    {
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
        public bool IsNewCategory { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? TransactionDate { get; set; }
        public decimal Confidence { get; set; }
        public bool Approved { get; set; } = true;
    }

    public class AiConfirmImportRequest
    {
        public int DiaryId { get; set; }
        public List<PendingTransactionModel> ApprovedTransactions { get; set; } = [];
        public List<string> ApprovedNewCategories { get; set; } = [];
    }
}
