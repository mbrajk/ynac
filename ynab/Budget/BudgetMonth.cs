using System.Text.Json.Serialization;

namespace ynab.Budget
{
    public class BudgetMonth
    {
        [JsonPropertyName("note")]
        public string Note { get; init; } = string.Empty;

        [JsonPropertyName("age_of_money")]
        public int? AgeOfMoney { get; init; } = 0;

        [JsonPropertyName("to_be_budgeted")]
        public decimal ToBeBudgeted { get; init; }
    }

    public class BudgetMonthResponse
    {
        [JsonPropertyName("month")]
        public required BudgetMonth Budget { get; init; }
    }
}
