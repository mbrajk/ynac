using System.Text.Json.Serialization;

namespace ynab.ScheduledTransaction
{
    public class ScheduledTransaction
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [JsonPropertyName("date_first")]
        public DateOnly DateFirst { get; init; }

        [JsonPropertyName("date_next")]
        public DateOnly DateNext { get; init; }

        [JsonPropertyName("frequency")]
        public string Frequency { get; init; } = string.Empty;

        [JsonPropertyName("amount")]
        public decimal Amount { get; init; }

        [JsonPropertyName("memo")]
        public string? Memo { get; init; }

        [JsonPropertyName("flag_color")]
        public string? FlagColor { get; init; }

        [JsonPropertyName("account_id")]
        public Guid AccountId { get; init; }

        [JsonPropertyName("account_name")]
        public string AccountName { get; init; } = string.Empty;

        [JsonPropertyName("payee_id")]
        public Guid? PayeeId { get; init; }

        [JsonPropertyName("payee_name")]
        public string? PayeeName { get; init; }

        [JsonPropertyName("category_id")]
        public Guid? CategoryId { get; init; }

        [JsonPropertyName("category_name")]
        public string? CategoryName { get; init; }

        [JsonPropertyName("transfer_account_id")]
        public Guid? TransferAccountId { get; init; }

        [JsonPropertyName("deleted")]
        public bool Deleted { get; init; }
    }

    public class ScheduledTransactionsResponse
    {
        [JsonPropertyName("scheduled_transactions")]
        public IReadOnlyCollection<ScheduledTransaction> ScheduledTransactions { get; init; } = Array.Empty<ScheduledTransaction>();

        [JsonPropertyName("server_knowledge")]
        public long ServerKnowledge { get; init; }
    }

    public class ScheduledTransactionResponse
    {
        [JsonPropertyName("scheduled_transaction")]
        public ScheduledTransaction? ScheduledTransaction { get; init; }
    }

    /// <summary>
    /// A scheduled transaction create/update payload. Null properties are omitted from the request,
    /// which leaves the corresponding fields unchanged on update.
    /// </summary>
    public class SaveScheduledTransaction
    {
        [JsonPropertyName("account_id")]
        public Guid? AccountId { get; init; }

        [JsonPropertyName("date")]
        public DateOnly? Date { get; init; }

        [JsonPropertyName("amount")]
        public decimal? Amount { get; init; }

        [JsonPropertyName("payee_id")]
        public Guid? PayeeId { get; init; }

        [JsonPropertyName("payee_name")]
        public string? PayeeName { get; init; }

        [JsonPropertyName("category_id")]
        public Guid? CategoryId { get; init; }

        [JsonPropertyName("memo")]
        public string? Memo { get; init; }

        [JsonPropertyName("flag_color")]
        public string? FlagColor { get; init; }

        [JsonPropertyName("frequency")]
        public string? Frequency { get; init; }
    }

    public class SaveScheduledTransactionRequest
    {
        [JsonPropertyName("scheduled_transaction")]
        public required SaveScheduledTransaction ScheduledTransaction { get; init; }
    }
}
