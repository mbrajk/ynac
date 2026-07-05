using System.Text.Json.Serialization;

namespace ynab.Transaction
{
    /// <summary>
    /// Well-known values for a transaction's cleared status.
    /// </summary>
    public static class TransactionClearedStatus
    {
        public const string Cleared = "cleared";
        public const string Uncleared = "uncleared";
        public const string Reconciled = "reconciled";
    }

    public class Transaction
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = string.Empty;

        [JsonPropertyName("date")]
        public DateOnly Date { get; init; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; init; }

        [JsonPropertyName("memo")]
        public string? Memo { get; init; }

        [JsonPropertyName("cleared")]
        public string Cleared { get; init; } = TransactionClearedStatus.Uncleared;

        [JsonPropertyName("approved")]
        public bool Approved { get; init; }

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

        [JsonPropertyName("transfer_transaction_id")]
        public string? TransferTransactionId { get; init; }

        [JsonPropertyName("import_id")]
        public string? ImportId { get; init; }

        [JsonPropertyName("import_payee_name")]
        public string? ImportPayeeName { get; init; }

        [JsonPropertyName("deleted")]
        public bool Deleted { get; init; }

        [JsonPropertyName("subtransactions")]
        public IReadOnlyCollection<SubTransaction> SubTransactions { get; init; } = Array.Empty<SubTransaction>();
    }

    public class SubTransaction
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = string.Empty;

        [JsonPropertyName("transaction_id")]
        public string TransactionId { get; init; } = string.Empty;

        [JsonPropertyName("amount")]
        public decimal Amount { get; init; }

        [JsonPropertyName("memo")]
        public string? Memo { get; init; }

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
}
