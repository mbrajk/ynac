using YnabApi.Budget;

namespace ynac.OSFeatures;

internal interface IBudgetOpener
{
    void OpenBudget(Budget selectedBudget);
}