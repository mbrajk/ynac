using Spectre.Console;
using YnabApi.Budget;

namespace ynac;

public interface IBudgetSelector
{
    public Task<Budget> SelectBudget(string budgetFilter, bool selectLastBudget = true);
}