using System.Text.Json.Serialization;

namespace ynab.Transaction;

public class Transaction
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("date")]
    public string Date { get; init; } = string.Empty;

    [JsonPropertyName("amount")]
    public long Amount { get; init; }

    [JsonPropertyName("memo")]
    public string? Memo { get; init; }

    [JsonPropertyName("cleared")]
    public string Cleared { get; init; } = string.Empty;

    [JsonPropertyName("approved")]
    public bool Approved { get; init; }

    [JsonPropertyName("flag_color")]
    public string? FlagColor { get; init; }

    [JsonPropertyName("account_id")]
    public string AccountId { get; init; } = string.Empty;

    [JsonPropertyName("account_name")]
    public string AccountName { get; init; } = string.Empty;

    [JsonPropertyName("payee_id")]
    public string? PayeeId { get; init; }

    [JsonPropertyName("payee_name")]
    public string? PayeeName { get; init; }

    [JsonPropertyName("category_id")]
    public string? CategoryId { get; init; }

    [JsonPropertyName("category_name")]
    public string? CategoryName { get; init; }

    [JsonPropertyName("transfer_account_id")]
    public string? TransferAccountId { get; init; }

    [JsonPropertyName("transfer_transaction_id")]
    public string? TransferTransactionId { get; init; }

    [JsonPropertyName("deleted")]
    public bool Deleted { get; init; }
}
