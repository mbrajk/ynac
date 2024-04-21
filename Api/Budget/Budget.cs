using System.Text.Json.Serialization;

namespace YnabApi.Budget
{
    public class Budget
    {
        [JsonPropertyName("name")]
        public string Name { get; init; }

        public Guid Id { get; init; }
    }
}
