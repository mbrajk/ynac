using Spectre.Console;
using YnabApi.Budget;

namespace ynac.BudgetSelection;

public class BudgetPrompter : IBudgetPrompter
{
    public Budget PromptBudgetSelection(IReadOnlyCollection<Budget> budgets)
    {
        if (budgets.Count == 0)
        {
            return Budget.NoBudget;
        }
        
        // if there is only one budget, select it automatically
        if (budgets.Count == 1)
        {
            return budgets.First();
        }
            		
        var selectedBudget = AnsiConsole.Prompt(
            new SelectionPrompt<Budget>()
                .Title("[italic grey]Select a[/] [italic aqua]budget:[/]")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more budgets)[/]")
                .AddChoices(budgets)
                .UseConverter(budget => $"{budget} [grey]{budget.BudgetId}[/]")
        );
	     
        return selectedBudget;
    }
}