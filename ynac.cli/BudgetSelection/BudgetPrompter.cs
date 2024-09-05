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

public interface IPrompter
{
    T PromptSelection<T>(IReadOnlyCollection<T> items, string title, Func<T, string> converter) where T : notnull;
}

public abstract class PrompterBase : IPrompter
{
    private readonly IConsolePrompt _console;

    protected PrompterBase(IConsolePrompt console)
    {
        _console = console;
    }

    public T PromptSelection<T>(IReadOnlyCollection<T> items, string title, Func<T, string> converter) where T : notnull
    {
        if (items.Count == 0)
        {
            throw new ArgumentException("No items to select from.", nameof(items));
        }

        if (items.Count == 1)
        {
            return items.First();
        }

        return _console.Prompt(
            new SelectionPrompt<T>()
                .Title(title)
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]")
                .AddChoices(items)
                .UseConverter(converter)
        );
    }
}

public class BudgetPrompter : PrompterBase, IBudgetPrompter
{
    public BudgetPrompter(IConsolePrompt console) 
        : base(console)
    { }
        
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
            		
        return PromptSelection(budgets, "[italic grey]Select a[/] [italic aqua]budget:[/]", budget => $"{budget} [grey]{budget.BudgetId}[/]");
    }
}