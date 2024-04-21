using System.Text.Json.Serialization;

namespace YnabApi.Budget
{
    public class BudgetResponse
    {
        [JsonPropertyName("budgets")]
        public IReadOnlyCollection<Budget> Budgets { get; init; }
    }
}
