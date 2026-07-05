using System.Text.Json.Serialization;

namespace ynab.Transaction
{
    /// <summary>
    /// A transaction create/update payload. Null properties are omitted from the request,
    /// which leaves the corresponding fields unchanged on update.
    /// </summary>
    public class SaveTransaction
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

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

        [JsonPropertyName("cleared")]
        public string? Cleared { get; init; }

        [JsonPropertyName("approved")]
        public bool? Approved { get; init; }

        [JsonPropertyName("flag_color")]
        public string? FlagColor { get; init; }

        [JsonPropertyName("import_id")]
        public string? ImportId { get; init; }
    }

    public class SaveTransactionRequest
    {
        [JsonPropertyName("transaction")]
        public required SaveTransaction Transaction { get; init; }
    }

    public class SaveTransactionsRequest
    {
        [JsonPropertyName("transactions")]
        public required IReadOnlyCollection<SaveTransaction> Transactions { get; init; }
    }
}
