using System.Text.Json.Serialization;

namespace ynab.Account
{
    public class Account
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;

        [JsonPropertyName("on_budget")]
        public bool OnBudget { get; init; }

        [JsonPropertyName("closed")]
        public bool Closed { get; init; }

        [JsonPropertyName("note")]
        public string? Note { get; init; }

        [JsonPropertyName("balance")]
        public decimal Balance { get; init; }

        [JsonPropertyName("cleared_balance")]
        public decimal ClearedBalance { get; init; }

        [JsonPropertyName("uncleared_balance")]
        public decimal UnclearedBalance { get; init; }

        [JsonPropertyName("transfer_payee_id")]
        public Guid? TransferPayeeId { get; init; }

        [JsonPropertyName("direct_import_linked")]
        public bool DirectImportLinked { get; init; }

        [JsonPropertyName("direct_import_in_error")]
        public bool DirectImportInError { get; init; }

        [JsonPropertyName("last_reconciled_at")]
        public DateTimeOffset? LastReconciledAt { get; init; }

        [JsonPropertyName("deleted")]
        public bool Deleted { get; init; }
    }
}
