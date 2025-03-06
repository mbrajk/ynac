using System.Text.Json.Serialization;

namespace ynab.Budget
{
    public class BudgetResponse
    {
        [JsonPropertyName("budgets")]
        public IReadOnlyCollection<Budget> Budgets { get; init; } = Array.Empty<Budget>();
    }
}
