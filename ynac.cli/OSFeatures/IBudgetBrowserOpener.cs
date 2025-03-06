using ynab.Budget;

namespace ynac.OSFeatures;

internal interface IBudgetBrowserOpener
{
    void OpenBudget(Budget selectedBudget);
}