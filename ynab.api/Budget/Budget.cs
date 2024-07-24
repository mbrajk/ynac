using System.Text.Json.Serialization;

namespace YnabApi.Budget
{
    public class Budget
    {
        public static readonly Budget LastUsedBudget;
        public static readonly Budget NoBudget;
        
        // TODO: separate out the visible budget name from the budget name used internally for YNAB API calls
        private static string LastUsedBudgetName => "last-used";
        private static string NoBudgetsFoundText => "ynac:error:no-budgets-found";
        private static string LastUsedBudgetDescription => "Last Used Budget";
        
        static Budget()
        {
            NoBudget = new Budget
            {
                Name = NoBudgetsFoundText,
                Id = Guid.Empty, 
                Type = BudgetType.NotFound
            };
            
            LastUsedBudget = new Budget
            {
                Name = LastUsedBudgetName,
                Id = Guid.Empty, 
                Type = BudgetType.LastUsed
            };
        }
        
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        public Guid Id { get; init; }
        
        public BudgetType Type { get; init; } = BudgetType.UserBudget;

        public string BudgetId
        {
            get
            {
                if (Type == BudgetType.LastUsed)
                    return LastUsedBudgetName;
                return Id.ToString();
            }
        }

        public override string ToString()
        {
            if (Type == BudgetType.LastUsed)
                return LastUsedBudgetDescription;
            return Name;
        }
    }

    public enum BudgetType
    {
        UserBudget,
        LastUsed,
        NotFound,
    }
}
