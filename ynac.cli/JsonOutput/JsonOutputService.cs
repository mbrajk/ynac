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
    /// Outputs budget data as JSON to a file or stdout
    /// </summary>
    /// <param name="budget">The selected budget</param>
    /// <param name="budgetMonth">Budget month data</param>
    /// <param name="categoryGroups">Category groups data</param>
    /// <param name="outputPath">Optional path to write JSON file. If null, writes to stdout</param>
    Task OutputBudgetJsonAsync(Budget budget, BudgetMonth budgetMonth, IReadOnlyCollection<CategoryGroup> categoryGroups, string? outputPath);
}

public class JsonOutputService : IJsonOutputService
{
    public async Task OutputBudgetJsonAsync(
        Budget budget,
        BudgetMonth budgetMonth,
        IReadOnlyCollection<CategoryGroup> categoryGroups,
        string? outputPath)
    {
        var output = new BudgetJsonOutput
        {
            BudgetName = budget.Name,
            BudgetId = budget.BudgetId,
            AgeOfMoney = budgetMonth.AgeOfMoney,
            ToBeBudgeted = budgetMonth.ToBeBudgeted,
            CategoryGroups = categoryGroups
        };

        var json = JsonSerializer.Serialize(output, YnacJsonSerializerContext.Default.BudgetJsonOutput);

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            // Output to stdout
            Console.WriteLine(json);
        }
        else
        {
            // Output to file
            var fullPath = Path.GetFullPath(outputPath);
            var directory = Path.GetDirectoryName(fullPath);
            
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            await File.WriteAllTextAsync(fullPath, json);
        }
    }
}
