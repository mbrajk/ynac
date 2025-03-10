using ynab.Budget;

namespace ynac.OSFeatures;

internal class BudgetBrowserOpener : IBudgetBrowserOpener
{
    private readonly IBrowserOpener _browserOpener;

    public BudgetBrowserOpener(IBrowserOpener browserOpener)
    {
        _browserOpener = browserOpener;
    }

    /*
     * Opens the selected budget in the default browser, currently only implemented for Windows
     * other OSes are not implemented and will likely throw an unhandled exception
     *
     * @param selectedBudget the budget to open
     */
    public void OpenBudget(Budget selectedBudget)
    {
        _browserOpener.OpenBrowser($"{Constants.YnabRootUrl}{selectedBudget.Id}{Constants.BudgetRouteAffix}");
    }
}