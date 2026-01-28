using System.Text.Json;
using ynab.Budget;
using ynab.Category;

namespace ynac.JsonOutput;

public interface IJsonOutputWriter
{
    void Write(Budget budget, BudgetMonth budgetMonth, IReadOnlyCollection<CategoryGroup> categoryGroups);
}

public class JsonOutputWriter : IJsonOutputWriter
{
    public void Write(Budget budget, BudgetMonth budgetMonth, IReadOnlyCollection<CategoryGroup> categoryGroups)
    {
        var output = BuildOutput(budget, budgetMonth, categoryGroups);
        var json = JsonSerializer.Serialize(output, YnacJsonSerializerContext.Default.BudgetOutput);
        Console.WriteLine(json);
    }

    internal static BudgetOutput BuildOutput(Budget budget, BudgetMonth budgetMonth, IReadOnlyCollection<CategoryGroup> categoryGroups)
    {
        var budgetSummary = new BudgetSummaryOutput(
            Name: budget.Name,
            Id: budget.Id.ToString(),
            AgeOfMoney: budgetMonth.AgeOfMoney,
            ToBeBudgeted: budgetMonth.ToBeBudgeted / 1000m
        );

        var categoryGroupOutputs = categoryGroups
            .Select(BuildCategoryGroupOutput)
            .ToList();

        return new BudgetOutput(budgetSummary, categoryGroupOutputs);
    }

    internal static CategoryGroupOutput BuildCategoryGroupOutput(CategoryGroup group)
    {
        var categories = group.Categories
            .Where(c => !c.Deleted)
            .Select(BuildCategoryOutput)
            .ToList();

        var totals = new CategoryTotalsOutput(
            Budgeted: categories.Sum(c => c.Budgeted),
            Activity: categories.Sum(c => c.Activity),
            Available: categories.Sum(c => c.Available)
        );

        return new CategoryGroupOutput(
            Name: group.Name,
            Id: group.Id.ToString(),
            Totals: totals,
            Categories: categories
        );
    }

    internal static CategoryOutput BuildCategoryOutput(Category category)
    {
        return new CategoryOutput(
            Name: category.Name,
            Id: category.Id.ToString(),
            Budgeted: category.Budgeted / 1000m,
            Activity: category.Activity / 1000m,
            Available: category.Balance / 1000m,
            GoalPercentageComplete: category.GoalPercentageComplete
        );
    }
}
