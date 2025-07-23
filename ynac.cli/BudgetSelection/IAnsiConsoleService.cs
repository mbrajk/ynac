using Spectre.Console;
using Spectre.Console.Rendering;

namespace ynac.BudgetSelection;

public interface IAnsiConsoleService
{
    void Markup(string value);
    void WriteLine(string value);
    void Write(IRenderable value);
    T Prompt<T>(IPrompt<T> prompt);
    void WriteHeaderRule(string title);
}
