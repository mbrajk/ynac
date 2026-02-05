using System.Text.Json.Serialization;

namespace ynab.Transaction;

public class UpdateTransactionResponse
{
    [JsonPropertyName("transaction")]
    public Transaction Transaction { get; init; } = new();
}
