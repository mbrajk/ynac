using Spectre.Console;
using YnabApi.Budget;

namespace ynac.BudgetSelection;

public interface IConsolePrompt
{
    T Prompt<T>(SelectionPrompt<T> prompt) where T : notnull;
}

public class ConsolePrompt : IConsolePrompt
{
    private readonly IAnsiConsole _console;

    public ConsolePrompt(IAnsiConsole console)
    {
        _console = console;
    }

    public T Prompt<T>(SelectionPrompt<T> prompt) where T : notnull
    {
        return _console.Prompt(prompt);
    }
}

public class BudgetPrompter : IBudgetPrompter
{
    private readonly IConsolePrompt _console;
    public BudgetPrompter(IConsolePrompt console)
    {
        _console = console;
    }
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
            		
        var selectedBudget = _console.Prompt(
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