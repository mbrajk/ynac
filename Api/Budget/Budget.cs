using System.Text.Json.Serialization;

namespace YnabApi.Budget
{
    public class Budget
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        public Guid Id { get; init; }

        public string BudgetId
        {
            get
            {
                if (Name == "last-used" && Id == Guid.Empty)
                    return "last-used";
                return Id.ToString();
            }
        }

        public override string ToString()
        {
            if (Name == "last-used" && Id == Guid.Empty)
                    return "Last Used Budget";
            return Name;
        }
    }
}
