using System.Text.Json.Serialization;

namespace ynab.Transaction;

public class TransactionResponse
{ 
    [JsonPropertyName("budgets")]
    public IReadOnlyCollection<Transaction> Transactions { get; init; } = Array.Empty<Transaction>();
}