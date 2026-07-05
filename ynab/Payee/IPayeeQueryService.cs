namespace ynab.Payee;

/// <summary>
/// Provides query operations for retrieving payees from YNAB.
/// </summary>
public interface IPayeeQueryService
{
    /// <summary>
    /// Retrieves all payees for the given budget. Deleted payees are filtered out.
    /// </summary>
    /// <param name="budget">The budget to retrieve payees for.</param>
    /// <returns>A read-only collection of payees.</returns>
    Task<IReadOnlyCollection<Payee>> GetBudgetPayees(Budget.Budget budget);
}
