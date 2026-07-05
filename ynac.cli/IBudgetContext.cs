using ynab.Budget;

namespace ynac;

/// <summary>
/// Session state shared between the console loop and budget actions: which budget is
/// selected and whether an action has changed data on the server since the last render.
/// </summary>
public interface IBudgetContext
{
    /// <summary>
    /// The budget the user selected for this session.
    /// </summary>
    Budget SelectedBudget { get; set; }

    /// <summary>
    /// Set by actions that modify budget data on the server. When true, the console
    /// re-fetches budget data before the next render and resets the flag.
    /// </summary>
    bool DataStale { get; set; }
}
