using ynac.BudgetSelection;
using ynac.CurrencyFormatting;

namespace ynac.BudgetActions;

/// <summary>
/// Budget action that toggles the visibility of currency amounts between hidden and shown.
/// </summary>
public class ToggleHideAmountsBudgetAction : IBudgetAction
{
    private readonly ICurrencyVisibilityState _visibilityState;
    private readonly IAnsiConsoleService _console;

    public ToggleHideAmountsBudgetAction(
        ICurrencyVisibilityState visibilityState,
        IAnsiConsoleService console)
    {
        _visibilityState = visibilityState;
        _console = console;
    }

    public string DisplayName => _visibilityState.Hidden ? "Show amounts" : "Hide amounts";

    public int Order => 0;

    public void Execute()
    {
        _visibilityState.Hidden = !_visibilityState.Hidden;
    }
}
