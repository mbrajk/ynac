namespace ynab.ScheduledTransaction;

/// <summary>
/// Provides query operations for retrieving scheduled transactions from YNAB.
/// </summary>
public interface IScheduledTransactionQueryService
{
    /// <summary>
    /// Retrieves all scheduled transactions for the given budget, ordered by next occurrence.
    /// Deleted scheduled transactions are filtered out.
    /// </summary>
    /// <param name="budget">The budget to retrieve scheduled transactions for.</param>
    /// <returns>A read-only collection of scheduled transactions.</returns>
    Task<IReadOnlyCollection<ScheduledTransaction>> GetScheduledTransactions(Budget.Budget budget);
}
