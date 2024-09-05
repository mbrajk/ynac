using YnabApi.Budget;

namespace ynac.BudgetSelection;

public interface IBudgetPrompter : IPrompter
{
    public Budget PromptBudgetSelection(IReadOnlyCollection<Budget> budgets);
}