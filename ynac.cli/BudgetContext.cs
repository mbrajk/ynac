using ynab.Budget;

namespace ynac;

internal class BudgetContext : IBudgetContext
{
    public Budget SelectedBudget { get; set; } = Budget.NoBudget;

    public bool DataStale { get; set; }
}
