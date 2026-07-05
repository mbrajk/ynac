namespace ynab.ScheduledTransaction;

/// <summary>
/// Provides write operations for scheduled transactions in YNAB.
/// </summary>
public interface IScheduledTransactionCommandService
{
    /// <summary>
    /// Creates a scheduled transaction.
    /// </summary>
    /// <param name="budget">The budget to create the scheduled transaction in.</param>
    /// <param name="scheduledTransaction">The scheduled transaction to create. AccountId and Date are required by the API.</param>
    /// <returns>The created scheduled transaction, or null if creation failed.</returns>
    Task<ScheduledTransaction?> CreateScheduledTransaction(Budget.Budget budget, SaveScheduledTransaction scheduledTransaction);

    /// <summary>
    /// Updates an existing scheduled transaction. Null properties are left unchanged.
    /// </summary>
    /// <param name="budget">The budget the scheduled transaction belongs to.</param>
    /// <param name="scheduledTransactionId">The id of the scheduled transaction to update.</param>
    /// <param name="scheduledTransaction">The fields to update.</param>
    /// <returns>The updated scheduled transaction, or null if the update failed.</returns>
    Task<ScheduledTransaction?> UpdateScheduledTransaction(Budget.Budget budget, Guid scheduledTransactionId, SaveScheduledTransaction scheduledTransaction);

    /// <summary>
    /// Deletes a scheduled transaction.
    /// </summary>
    /// <param name="budget">The budget the scheduled transaction belongs to.</param>
    /// <param name="scheduledTransactionId">The id of the scheduled transaction to delete.</param>
    /// <returns>The deleted scheduled transaction, or null if deletion failed.</returns>
    Task<ScheduledTransaction?> DeleteScheduledTransaction(Budget.Budget budget, Guid scheduledTransactionId);
}
