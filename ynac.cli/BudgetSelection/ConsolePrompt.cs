using Spectre.Console;

namespace ynac.BudgetSelection;

public class ConsolePrompt : IConsolePrompt
{
    private readonly IAnsiConsole _console;

    public ConsolePrompt()
    {
        _console = AnsiConsole.Console;
    }

    public T Prompt<T>(SelectionPrompt<T> prompt) where T : notnull
    {
        return _console.Prompt(prompt);
    }
}