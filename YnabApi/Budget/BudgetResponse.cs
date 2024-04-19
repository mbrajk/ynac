using System.Text.Json.Serialization;

namespace ynac.YnabApi.Budget
{
    public class BudgetResponse
    {
        [JsonPropertyName("budgets")]
        public IReadOnlyCollection<Budget> Budgets { get; init; }
    }
}
