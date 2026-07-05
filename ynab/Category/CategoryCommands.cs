using System.Text.Json.Serialization;

namespace ynab.Category
{
    public class SingleCategoryResponse
    {
        [JsonPropertyName("category")]
        public Category? Category { get; init; }
    }

    /// <summary>
    /// Payload for updating a category's budgeted amount for a specific month.
    /// </summary>
    public class SaveMonthCategory
    {
        [JsonPropertyName("budgeted")]
        public decimal Budgeted { get; init; }
    }

    public class SaveMonthCategoryRequest
    {
        [JsonPropertyName("category")]
        public required SaveMonthCategory Category { get; init; }
    }

    /// <summary>
    /// Payload for updating a category's details. Null properties are left unchanged.
    /// </summary>
    public class SaveCategory
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("note")]
        public string? Note { get; init; }

        [JsonPropertyName("category_group_id")]
        public Guid? CategoryGroupId { get; init; }
    }

    public class SaveCategoryRequest
    {
        [JsonPropertyName("category")]
        public required SaveCategory Category { get; init; }
    }
}
