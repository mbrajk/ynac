using Spectre.Console;

namespace ynac.BudgetSelection;

public interface IConsolePrompt
{
    T Prompt<T>(SelectionPrompt<T> prompt) where T : notnull;
}