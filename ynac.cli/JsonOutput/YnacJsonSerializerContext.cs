using System.Text.Json.Serialization;
using ynab.Budget;
using ynab.Category;

namespace ynac.JsonOutput;

[JsonSerializable(typeof(BudgetJsonOutput))]
[JsonSerializable(typeof(BudgetMonth))]
[JsonSerializable(typeof(CategoryGroup))]
[JsonSerializable(typeof(Category))]
[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
internal partial class YnacJsonSerializerContext : JsonSerializerContext
{
}
