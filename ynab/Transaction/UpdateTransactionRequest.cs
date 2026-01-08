using System.Text.Json.Serialization;

namespace ynab.Transaction;

public class UpdateTransactionRequest
{
    [JsonPropertyName("transaction")]
    public UpdateTransactionData Transaction { get; init; } = new();
}

public class UpdateTransactionData
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("approved")]
    public bool Approved { get; init; }
}
