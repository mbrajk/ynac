using System.Diagnostics;
using YnabApi.Budget;

namespace ynac;

static class OSFeatures
{
    public static void OpenBudgetWindows(Budget selectedBudget)
    {
        //only works on windows but is possible on Linux and Mac
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = $"{Constants.YnabRootUrl}{selectedBudget.Id}{Constants.BudgetRouteAffix}",
            UseShellExecute = true
        };
        Process.Start (psi);
    }

    public static void OpenBudgetLinux(Budget selectedBudget)
    {
        throw new NotImplementedException();
    }

    private static void OpenBudgetMac(Budget selectedBudget)
    {
        throw new NotImplementedException();
    }
}