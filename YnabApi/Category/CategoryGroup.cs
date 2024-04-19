using System.Text.Json.Serialization;

namespace ynac.YnabApi.Category
{
    public class CategoryGroup
    {
        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [JsonPropertyName("hidden")]
        public bool Hidden { get; init; }

        [JsonPropertyName("deleted")]
        public bool Deleted { get; init; }

        [JsonPropertyName("categories")]
        public IReadOnlyCollection<Category> Categories { get; init; }
    }
}
