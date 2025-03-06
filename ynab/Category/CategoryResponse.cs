using System.Text.Json.Serialization;

namespace ynab.Category
{
    public class CategoryResponse
    {
        [JsonPropertyName("category_groups")]
        public IReadOnlyCollection<CategoryGroup>? Groups { get; init; }
    }
}
