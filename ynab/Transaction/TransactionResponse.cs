using System.Text.Json.Serialization;

namespace ynab.Transaction;

public class TransactionResponse
{
    [JsonPropertyName("transactions")]
    public List<Transaction> Transactions { get; init; } = [];
}
