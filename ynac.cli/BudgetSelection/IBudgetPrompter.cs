using YnabApi.Budget;

namespace ynac;

public interface IBudgetPrompter
{
    public Budget PromptBudgetSelection(IReadOnlyCollection<Budget> budgets);
}