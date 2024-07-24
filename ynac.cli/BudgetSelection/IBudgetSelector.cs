using YnabApi.Budget;

namespace ynac.BudgetSelection;

public interface IBudgetSelector
{
    public Task<Budget> SelectBudget(string budgetFilter, bool selectLastBudget = true);
}