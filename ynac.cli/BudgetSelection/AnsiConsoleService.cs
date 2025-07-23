using Spectre.Console;
using Spectre.Console.Rendering;

namespace ynac.BudgetSelection;

internal class AnsiConsoleService : IAnsiConsoleService
{
    private readonly IAnsiConsole _console;
    
    public AnsiConsoleService()
    {
        _console = AnsiConsole.Console;
    }
    
    public void Markup(string value) => _console.Markup(value);
    
    public void WriteLine(string value) => _console.WriteLine(value);
    public void Write(IRenderable value) => _console.Write(value);
    
    public T Prompt<T>(IPrompt<T> prompt) => _console.Prompt(prompt);
    
    public void WriteHeaderRule(string title)
    {
        var rule = new Rule(title);
        rule.Style = Style.Parse("aqua");
        rule.LeftJustified();
        Write(rule);
    }
}
