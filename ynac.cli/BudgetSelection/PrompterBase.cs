using Spectre.Console;

namespace ynac.BudgetSelection;

public abstract class PrompterBase(IAnsiConsoleService console) : IPrompter
{
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

        return console.Prompt(
            new SelectionPrompt<T>()
                .Title(title)
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]")
                .AddChoices(items)
                .UseConverter(converter)
        );
    }
}