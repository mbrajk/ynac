using System.Text.Json.Serialization;

namespace YnabApi.Budget
{
    public class BudgetMonth
    {
        [JsonPropertyName("note")]
        public string Note { get; init; }

        [JsonPropertyName("age_of_money")]
        public int? AgeOfMoney { get; init; } = 0;
        
        [JsonPropertyName("to_be_budgeted")]
        public decimal ToBeBudgeted { get; init; }
    }
    
    public class BudgetMonthResponse
    {
        [JsonPropertyName("month")]
        public BudgetMonth Budget { get; init; }
    }
}
