using System.Text.Json.Serialization;

namespace ynab.Budget
{
    public class BudgetMonth
    {
        [JsonPropertyName("month")]
        public DateOnly Month { get; init; }

        [JsonPropertyName("note")]
        public string Note { get; init; } = string.Empty;

        [JsonPropertyName("income")]
        public decimal Income { get; init; }

        [JsonPropertyName("budgeted")]
        public decimal Budgeted { get; init; }

        [JsonPropertyName("activity")]
        public decimal Activity { get; init; }

        [JsonPropertyName("age_of_money")]
        public int? AgeOfMoney { get; init; } = 0;

        [JsonPropertyName("to_be_budgeted")]
        public decimal ToBeBudgeted { get; init; }

        [JsonPropertyName("deleted")]
        public bool Deleted { get; init; }
    }

    public class BudgetMonthResponse
    {
        [JsonPropertyName("month")]
        public required BudgetMonth Budget { get; init; }
    }

    public class BudgetMonthsResponse
    {
        [JsonPropertyName("months")]
        public IReadOnlyCollection<BudgetMonth> Months { get; init; } = Array.Empty<BudgetMonth>();

        [JsonPropertyName("server_knowledge")]
        public long ServerKnowledge { get; init; }
    }
}
