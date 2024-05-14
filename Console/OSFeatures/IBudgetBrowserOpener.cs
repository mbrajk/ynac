using YnabApi.Budget;

namespace ynac.OSFeatures;

internal interface IBudgetBrowserOpener
{
    void OpenBudget(Budget selectedBudget);
}