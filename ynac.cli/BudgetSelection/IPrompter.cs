namespace ynac.BudgetSelection;

public interface IPrompter
{
    T PromptSelection<T>(IReadOnlyCollection<T> items, string title, Func<T, string> converter) where T : notnull;
}