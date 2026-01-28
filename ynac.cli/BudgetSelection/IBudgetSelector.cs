using ynab.Budget;

namespace ynac.BudgetSelection;

public interface IBudgetSelector
{
    public Task<Budget> SelectBudget(string budgetFilter, bool selectLastBudget = true, bool nonInteractive = false);
}