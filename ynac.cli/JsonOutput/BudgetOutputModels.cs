using System.Text.Json.Serialization;

namespace ynac.JsonOutput;

public record BudgetOutput(
    [property: JsonPropertyName("budget")] BudgetSummaryOutput Budget,
    [property: JsonPropertyName("category_groups")] IReadOnlyCollection<CategoryGroupOutput> CategoryGroups
);

public record BudgetSummaryOutput(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("age_of_money")] int? AgeOfMoney,
    [property: JsonPropertyName("to_be_budgeted")] decimal ToBeBudgeted
);

public record CategoryGroupOutput(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("totals")] CategoryTotalsOutput Totals,
    [property: JsonPropertyName("categories")] IReadOnlyCollection<CategoryOutput> Categories
);

public record CategoryTotalsOutput(
    [property: JsonPropertyName("budgeted")] decimal Budgeted,
    [property: JsonPropertyName("activity")] decimal Activity,
    [property: JsonPropertyName("available")] decimal Available
);

public record CategoryOutput(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("budgeted")] decimal Budgeted,
    [property: JsonPropertyName("activity")] decimal Activity,
    [property: JsonPropertyName("available")] decimal Available,
    [property: JsonPropertyName("goal_percentage_complete")] int? GoalPercentageComplete
);
