using YnabApi.Budget;

namespace ynac.BudgetSelection;

public interface IBudgetPrompter
{
    public Budget PromptBudgetSelection(IReadOnlyCollection<Budget> budgets);
}