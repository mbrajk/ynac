using System.Text.Json;
using ynab.Budget;
using ynab.Category;

namespace ynac.JsonOutput;

/// <summary>
/// Service for outputting budget data as JSON
/// </summary>
public interface IJsonOutputService
{
    /// <summary>
    /// Outputs budget data as JSON to stdout
    /// </summary>
    /// <param name="budget">The selected budget</param>
    /// <param name="budgetMonth">Budget month data</param>
    /// <param name="categoryGroups">Category groups data</param>
    /// <param name="outputPath">Unused parameter (kept for compatibility)</param>
    Task OutputBudgetJsonAsync(Budget budget, BudgetMonth budgetMonth, IReadOnlyCollection<CategoryGroup> categoryGroups, string? outputPath);
}

public class JsonOutputService : IJsonOutputService
{
    private static decimal ConvertFromMilliunits(decimal milliunits) => milliunits / 1000m;

    public async Task OutputBudgetJsonAsync(
        Budget budget,
        BudgetMonth budgetMonth,
        IReadOnlyCollection<CategoryGroup> categoryGroups,
        string? outputPath)
    {
        // Convert category currency values from milliunits to dollars
        var normalizedCategoryGroups = categoryGroups.Select(group => new CategoryGroup
        {
            Id = group.Id,
            Name = group.Name,
            Hidden = group.Hidden,
            Deleted = group.Deleted,
            Categories = group.Categories.Select(cat => new Category
            {
                Id = cat.Id,
                CategoryGroupId = cat.CategoryGroupId,
                Name = cat.Name,
                Hidden = cat.Hidden,
                Note = cat.Note,
                Budgeted = ConvertFromMilliunits(cat.Budgeted),
                Activity = ConvertFromMilliunits(cat.Activity),
                Balance = ConvertFromMilliunits(cat.Balance),
                Deleted = cat.Deleted,
                GoalPercentageComplete = cat.GoalPercentageComplete
            }).ToList()
        }).ToList();

        var output = new BudgetJsonOutput
        {
            BudgetName = budget.Name,
            BudgetId = budget.BudgetId,
            AgeOfMoney = budgetMonth.AgeOfMoney,
            ToBeBudgeted = ConvertFromMilliunits(budgetMonth.ToBeBudgeted),
            CategoryGroups = normalizedCategoryGroups
        };

        var json = JsonSerializer.Serialize(output, YnacJsonSerializerContext.Default.BudgetJsonOutput);

        // Always output to stdout
        Console.WriteLine(json);
    }
}
