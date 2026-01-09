using System.Text.Json.Serialization;
using ynab.Budget;
using ynab.Category;

namespace ynac.JsonOutput;

/// <summary>
/// Represents the complete budget data structure for JSON output
/// </summary>
public class BudgetJsonOutput
{
    [JsonPropertyName("budget_name")]
    public string BudgetName { get; init; } = string.Empty;
    
    [JsonPropertyName("budget_id")]
    public string BudgetId { get; init; } = string.Empty;
    
    [JsonPropertyName("age_of_money")]
    public int? AgeOfMoney { get; init; }
    
    [JsonPropertyName("to_be_budgeted")]
    public decimal ToBeBudgeted { get; init; }
    
    [JsonPropertyName("category_groups")]
    public IReadOnlyCollection<CategoryGroup> CategoryGroups { get; init; } = Array.Empty<CategoryGroup>();
}
