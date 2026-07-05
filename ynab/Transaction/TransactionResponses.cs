using System.Text.Json.Serialization;

namespace ynab.Transaction
{
    public class TransactionsResponse
    {
        [JsonPropertyName("transactions")]
        public IReadOnlyCollection<Transaction> Transactions { get; init; } = Array.Empty<Transaction>();

        [JsonPropertyName("server_knowledge")]
        public long ServerKnowledge { get; init; }
    }

    public class TransactionResponse
    {
        [JsonPropertyName("transaction")]
        public Transaction? Transaction { get; init; }
    }

    /// <summary>
    /// Response for transaction creation, which may contain one or many created transactions.
    /// </summary>
    public class SaveTransactionsResponse
    {
        [JsonPropertyName("transaction_ids")]
        public IReadOnlyCollection<string> TransactionIds { get; init; } = Array.Empty<string>();

        [JsonPropertyName("transaction")]
        public Transaction? Transaction { get; init; }

        [JsonPropertyName("transactions")]
        public IReadOnlyCollection<Transaction>? Transactions { get; init; }

        [JsonPropertyName("duplicate_import_ids")]
        public IReadOnlyCollection<string>? DuplicateImportIds { get; init; }

        [JsonPropertyName("server_knowledge")]
        public long ServerKnowledge { get; init; }
    }

    public class TransactionImportResponse
    {
        [JsonPropertyName("transaction_ids")]
        public IReadOnlyCollection<string> TransactionIds { get; init; } = Array.Empty<string>();
    }
}
