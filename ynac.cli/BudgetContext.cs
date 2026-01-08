using ynab.Budget;

namespace ynac;

public interface IBudgetContext
{
    Budget? CurrentBudget { get; set; }
}

internal class BudgetContext : IBudgetContext
{
    public Budget? CurrentBudget { get; set; }
}
