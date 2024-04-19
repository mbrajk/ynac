using System.Text.Json.Serialization;

namespace ynac.YnabApi.Category
{
    public class Category
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [JsonPropertyName("category_group_id")]
        public Guid CategoryGroupId { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("hidden")]
        public bool Hidden { get; init; }

        [JsonPropertyName("note")]
        public string Note { get; init; }

        [JsonPropertyName("budgeted")]
        public decimal Budgeted { get; init; }

        [JsonPropertyName("activity")]
        public decimal Activity { get; init; }

        [JsonPropertyName("balance")]
        public decimal Balance { get; init; }

        [JsonPropertyName("deleted")]
        public bool Deleted { get; init; }
        
        [JsonPropertyName("goal_percentage_complete")]
        public int? GoalPercentageComplete { get; init; }
    }
}
